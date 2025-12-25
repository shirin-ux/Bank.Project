using Common;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Command.BuyPlan;
using LoanService.Application.UseCase.Investment.Query.OrderBuy;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoanService.Application.UseCase.Investment.Command.GiftCard;

/// <summary>
/// Handler for receiving gift card - manages the complete gift card flow:
/// 1. Validates eligibility and user data
/// 2. Creates insurance policy if needed (goes to Karizma)
/// 3. Processes order status and increases capital when ready
/// 4. Returns gift card details with amounts and gram equivalents
/// </summary>
public class ReceiveGiftCardCommandHandler(
    IGiftCardRepository giftCardRepository,
    IUserReadService userReadService,
    IInvestmentProvider investmentProvider,
    IInvestmentPlanReadRepository investmentPlanReadRepository,
    ILogger<ReceiveGiftCardCommandHandler> logger)
    : IRequestHandler<ReceiveGiftCardCommand, Result<ReceiveGiftCardResult>>
{
    private readonly IGiftCardRepository _giftCardRepository = giftCardRepository;
    private readonly IUserReadService _userReadService = userReadService;
    private readonly IInvestmentProvider _investmentProvider = investmentProvider;
    private readonly IInvestmentPlanReadRepository _investmentPlanReadRepository = investmentPlanReadRepository;
    private readonly ILogger<ReceiveGiftCardCommandHandler> _logger = logger;

    private const decimal GiftCardAmountRial = 15_000_000m;
    private const InvestmentPlanType GiftCardPlanType = InvestmentPlanType.Gold;

    public async Task<Result<ReceiveGiftCardResult>> Handle(ReceiveGiftCardCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("درخواست دریافت کارت هدیه برای کاربر {UserId}", request.UserId);

     
        var user = await _userReadService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("کاربر {UserId} یافت نشد", request.UserId);
            return Result<ReceiveGiftCardResult>.Failure(new Error(404, "کاربر یافت نشد."));
        }

     
        var eligibilityResult = await ValidateEligibilityAsync(request.UserId, user, cancellationToken);
        if (!eligibilityResult.IsSuccess)
        {
            return Result<ReceiveGiftCardResult>.Failure(eligibilityResult.Error!);
        }

     
        var giftCard = await GetOrCreateGiftCardAsync(request.UserId, cancellationToken);
        if (giftCard == null)
        {
            _logger.LogError("نمی‌توان GiftCard برای کاربر {UserId} ایجاد کرد", request.UserId);
            return Result<ReceiveGiftCardResult>.Failure(new Error(500, "خطای داخلی در ایجاد کارت هدیه"));
        }

    
        if (!giftCard.OrderId.HasValue)
        {
            _logger.LogInformation("کاربر {UserId} هنوز بیمه‌نامه ندارد. در حال ایجاد بیمه‌نامه از کاریزما...", request.UserId);
            return await CreatePolicyAndReturnPendingAsync(giftCard, user, cancellationToken);
        }

        // 5. Process existing order status
        return await ProcessExistingOrderAsync(giftCard, user, cancellationToken);
    }

    /// <summary>
    /// Validates user eligibility and required profile data
    /// </summary>
    private async Task<Result<Unit>> ValidateEligibilityAsync(Guid userId, UserInfoDto user, CancellationToken ct)
    {
        var isEligible = await _giftCardRepository.IsUserEligibleForGiftCardAsync(userId, ct);
        _logger.LogInformation(
            "بررسی واجد شرایط بودن کاربر {UserId}. IsEligible: {IsEligible}, GiftStatus: {GiftStatus}, NationalCode: {NationalCode}",
            userId, isEligible, user.GiftStatus, user.NationalCode);

        if (!isEligible)
        {
            _logger.LogWarning("کاربر {UserId} واجد شرایط دریافت کارت هدیه نیست. GiftStatus: {GiftStatus}", userId, user.GiftStatus);
            return Result<Unit>.Failure(new Error(403, "شما مجاز به دریافت کارت هدیه نیستید."));
        }

        // Validate required profile fields
        if (string.IsNullOrWhiteSpace(user.NationalCode))
        {
            return Result<Unit>.Failure(new Error(200, "کد ملی شما در سیستم ثبت نشده است. لطفاً ابتدا پروفایل خود را تکمیل کنید."));
        }

        if (string.IsNullOrWhiteSpace(user.PostalCode))
        {
            return Result<Unit>.Failure(new Error(200, "کد پستی شما در سیستم ثبت نشده است. لطفاً ابتدا پروفایل خود را تکمیل کنید."));
        }

        if (string.IsNullOrWhiteSpace(user.BirthDate))
        {
            return Result<Unit>.Failure(new Error(200, "تاریخ تولد شما در سیستم ثبت نشده است. لطفاً ابتدا پروفایل خود را تکمیل کنید."));
        }

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Gets existing GiftCard or creates a new one
    /// </summary>
    private async Task<Domain.Entities.Investment.GiftCard?> GetOrCreateGiftCardAsync(Guid userId, CancellationToken ct)
    {
        var existingGiftCard = await _giftCardRepository.GetByUserIdAsync(userId, ct);
        if (existingGiftCard != null)
        {
            return existingGiftCard;
        }

        _logger.LogInformation("ایجاد GiftCard جدید برای کاربر {UserId}", userId);

        var newGiftCard = new Domain.Entities.Investment.GiftCard(userId)
        {
            IsReceived = false,
            IsPending = false
        };

        try
        {
            existingGiftCard = await _giftCardRepository.CreateAsync(newGiftCard, ct);
            _logger.LogInformation("GiftCard با Id {GiftCardId} برای کاربر {UserId} ایجاد شد", existingGiftCard.Id, userId);
            return existingGiftCard;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "خطا در ایجاد GiftCard برای کاربر {UserId}. احتمالاً از جای دیگری ایجاد شده است", userId);
            // Try to get it again (race condition handling)
            return await _giftCardRepository.GetByUserIdAsync(userId, ct);
        }
    }

    /// <summary>
    /// Creates insurance policy for gift card (goes to Karizma) and returns pending status
    /// </summary>
    private async Task<Result<ReceiveGiftCardResult>> CreatePolicyAndReturnPendingAsync(
        Domain.Entities.Investment.GiftCard giftCard,
        UserInfoDto user,
        CancellationToken ct)
    {
        try
        {
            var buyPlanCommand = new BuyPlanCommand
            {
                PlanType = GiftCardPlanType
            };

            var policyResult = await _investmentProvider.CreatePolicyAndBuyAsync(
                buyPlanCommand,
                user.BirthDate,
                user.PostalCode,
                user.NationalCode,
                ct);

            if (!policyResult.IsSuccess)
            {
                _logger.LogError("خطا در ایجاد بیمه‌نامه از کاریزما برای کاربر {UserId}. Error: {Error}",
                    giftCard.UserId, policyResult.Error?.Message);
                return Result<ReceiveGiftCardResult>.Failure(
                    policyResult.Error ?? new Error(500, "خطا در ایجاد بیمه‌نامه از کاریزما"));
            }

            var orderResult = policyResult.Value!;
            if (!orderResult.OrderId.HasValue)
            {
                _logger.LogWarning("OrderId در نتیجه ایجاد بیمه‌نامه برای کاربر {UserId} موجود نیست", giftCard.UserId);
                return await BuildResultAsync(giftCard.Id, false, true,
                    "درخواست کارت هدیه شما ثبت شد. لطفاً صبر کنید.", ct);
            }

            // Update GiftCard with OrderId
            await _giftCardRepository.UpdateOrderIdAsync(giftCard.UserId, orderResult.OrderId.Value, ct);
            await _giftCardRepository.UpdateAsPendingAsync(giftCard.UserId, true, ct);

            _logger.LogInformation(
                "بیمه‌نامه برای کاربر {UserId} ایجاد شد. OrderId: {OrderId}, GiftCardId: {GiftCardId}",
                giftCard.UserId, orderResult.OrderId.Value, giftCard.Id);

            return await BuildResultAsync(giftCard.Id, false, true,
                "درخواست کارت هدیه شما ثبت شد و در حال پردازش است. لطفاً صبر کنید.", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای غیرمنتظره در ایجاد بیمه‌نامه برای کاربر {UserId}", giftCard.UserId);
            return Result<ReceiveGiftCardResult>.Failure(new Error(500, "خطا در ایجاد بیمه‌نامه. لطفاً بعداً تلاش کنید."));
        }
    }

    /// <summary>
    /// Processes existing order status and increases capital if needed
    /// </summary>
    private async Task<Result<ReceiveGiftCardResult>> ProcessExistingOrderAsync(
        Domain.Entities.Investment.GiftCard giftCard,
        UserInfoDto user,
        CancellationToken ct)
    {
        var orderId = giftCard.OrderId!.Value;
        _logger.LogInformation(
            "پردازش سفارش موجود. UserId: {UserId}, OrderId: {OrderId}, IsReceived: {IsReceived}, IsPending: {IsPending}",
            giftCard.UserId, orderId, giftCard.IsReceived, giftCard.IsPending);

        // Get order status from Karizma
        GetOrderStatusResultDto orderStatus;
        try
        {
            orderStatus = await _investmentProvider.GetOrderBuyByIdAsync(orderId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "خطا در دریافت وضعیت سفارش از کاریزما. OrderId: {OrderId}", orderId);
            return Result<ReceiveGiftCardResult>.Failure(
                new Error(200, "خطا در استعلام وضعیت سفارش. لطفاً بعداً تلاش کنید."));
        }

        _logger.LogInformation(
            "وضعیت سفارش دریافت شد. OrderId: {OrderId}, Status: {Status}, WealthPolicyId: {WealthPolicyId}",
            orderId, orderStatus.Status, orderStatus.WealthPolicyId);

        return orderStatus.Status switch
        {
            statusType.FinalStatus => await ProcessFinalStatusAsync(giftCard, orderStatus, user, ct),
            statusType.ProcessingStatus => await ProcessProcessingStatusAsync(giftCard, ct),
            statusType.CancelledStatus => await ProcessCancelledStatusAsync(giftCard, ct),
            _ => await ProcessUnknownStatusAsync(giftCard, orderStatus.Status, ct)
        };
    }

    /// <summary>
    /// Processes FinalStatus: increases capital and marks as received
    /// </summary>
    private async Task<Result<ReceiveGiftCardResult>> ProcessFinalStatusAsync(
        Domain.Entities.Investment.GiftCard giftCard,
        GetOrderStatusResultDto orderStatus,
        UserInfoDto user,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "پردازش وضعیت نهایی سفارش. UserId: {UserId}, OrderId: {OrderId}, WealthPolicyId: {WealthPolicyId}",
            giftCard.UserId, giftCard.OrderId, orderStatus.WealthPolicyId);

        if (!orderStatus.WealthPolicyId.HasValue)
        {
            _logger.LogWarning("WealthPolicyId موجود نیست. OrderId: {OrderId}", giftCard.OrderId);
            return Result<ReceiveGiftCardResult>.Failure(
                new Error(200, "اطلاعات بیمه‌نامه موجود نیست. لطفاً بعداً تلاش کنید."));
        }

        // Check if capital already increased
        var capitalIncreased = await IncreaseCapitalIfNeededAsync(
            giftCard.OrderId!.Value,
            orderStatus.WealthPolicyId.Value,
            user.NationalCode!,
            ct);

        if (!capitalIncreased.IsSuccess)
        {
            return Result<ReceiveGiftCardResult>.Failure(capitalIncreased.Error!);
        }

        // Mark as received if capital was increased or already received
        if (capitalIncreased.Value && !giftCard.IsReceived)
        {
            await _giftCardRepository.UpdateAsReceivedAsync(giftCard.UserId, giftCard.OrderId, ct);
            _logger.LogInformation("کارت هدیه برای کاربر {UserId} به‌عنوان دریافت شده علامت‌گذاری شد", giftCard.UserId);
        }

        var message = giftCard.IsReceived
            ? "کارت هدیه شما قبلاً دریافت شده است."
            : "کارت هدیه شما با موفقیت دریافت شد و آماده استفاده است.";

        return await BuildResultAsync(giftCard.Id, true, false, message, ct);
    }

    /// <summary>
    /// Processes ProcessingStatus: marks as pending
    /// </summary>
    private async Task<Result<ReceiveGiftCardResult>> ProcessProcessingStatusAsync(
        Domain.Entities.Investment.GiftCard giftCard,
        CancellationToken ct)
    {
        if (!giftCard.IsReceived && !giftCard.IsPending)
        {
            await _giftCardRepository.UpdateAsPendingAsync(giftCard.UserId, true, ct);
        }

        var message = giftCard.IsReceived
            ? "کارت هدیه شما قبلاً دریافت شده است."
            : "درخواست کارت هدیه شما در حال پردازش است. لطفاً صبر کنید.";

        return await BuildResultAsync(
            giftCard.Id,
            giftCard.IsReceived,
            !giftCard.IsReceived,
            message,
            ct);
    }

    /// <summary>
    /// Processes CancelledStatus: marks as not pending
    /// </summary>
    private async Task<Result<ReceiveGiftCardResult>> ProcessCancelledStatusAsync(
        Domain.Entities.Investment.GiftCard giftCard,
        CancellationToken ct)
    {
        _logger.LogWarning("سفارش لغو شده است. UserId: {UserId}, OrderId: {OrderId}",
            giftCard.UserId, giftCard.OrderId);

        await _giftCardRepository.UpdateAsPendingAsync(giftCard.UserId, false, ct);

        return await BuildResultAsync(
            giftCard.Id,
            false,
            false,
            "درخواست کارت هدیه شما لغو شده است.",
            ct);
    }

    /// <summary>
    /// Processes unknown status: keeps current state or marks as pending
    /// </summary>
    private async Task<Result<ReceiveGiftCardResult>> ProcessUnknownStatusAsync(
        Domain.Entities.Investment.GiftCard giftCard,
        statusType status,
        CancellationToken ct)
    {
        _logger.LogWarning("وضعیت نامشخص سفارش. UserId: {UserId}, OrderId: {OrderId}, Status: {Status}",
            giftCard.UserId, giftCard.OrderId, status);

        if (!giftCard.IsReceived)
        {
            await _giftCardRepository.UpdateAsPendingAsync(giftCard.UserId, true, ct);
            return await BuildResultAsync(giftCard.Id, false, true,
                "درخواست کارت هدیه شما در حال بررسی است.", ct);
        }

        return await BuildResultAsync(giftCard.Id, true, false,
            "کارت هدیه شما قبلاً دریافت شده است.", ct);
    }

    /// <summary>
    /// Increases capital if not already increased for this order
    /// </summary>
    private async Task<Result<bool>> IncreaseCapitalIfNeededAsync(
        Guid orderId,
        long wealthPolicyId,
        string nationalCode,
        CancellationToken ct)
    {
        // Get investment account
        //var investmentAccount = await _investmentPlanReadRepository.GetByNationalCodeAndPlanAsync(
        //    nationalCode,
        //    GiftCardPlanType,
        //    ct);

        //if (investmentAccount == null || !investmentAccount.ProviderPolicyId.HasValue)
        //{
        //    _logger.LogWarning("حساب سرمایه‌گذاری یافت نشد. NationalCode: {NationalCode}", nationalCode);
        //    return Result<bool>.Failure(new Error(404, "حساب سرمایه‌گذاری یافت نشد."));
        //}

        // Check if already increased
        //var expectedReceiptNumber = $"GIFT-{orderId:N}";
        //var alreadyIncreased = await _investmentPlanReadRepository.CheckReceiptNumberExistsAsync(wealthPolicyId,
        //    expectedReceiptNumber,
        //    ct);

        //if (alreadyIncreased)
        //{
        //    _logger.LogInformation("افزایش سرمایه قبلاً انجام شده است. OrderId: {OrderId}", orderId);
        //    return Result<bool>.Success(true);
        //}

        // Increase capital
        var receiptDate = DateTime.UtcNow;
        var receiptNumber = $"GIFT-{orderId:N}"; 
        var description = "افزایش سرمایه از طریق کارت هدیه";

        _logger.LogInformation(
            "در حال افزایش سرمایه. WealthPolicyId: {WealthPolicyId}, Amount: {Amount}, OrderId: {OrderId}",
            wealthPolicyId, GiftCardAmountRial, orderId);

        var increaseResult = await _investmentProvider.IncreaseCapitalDirectAsync(
            wealthPolicyId: wealthPolicyId,
            amount: GiftCardAmountRial,
            receiptNumber: receiptNumber,
            receiptDate: receiptDate,
            description: description,
            ct);

        if (!increaseResult.IsSuccess || increaseResult.Value == null)
        {
            _logger.LogError("خطا در افزایش سرمایه. WealthPolicyId: {WealthPolicyId}, Error: {Error}",
                wealthPolicyId, increaseResult.Error?.Message);
            return Result<bool>.Failure(
                increaseResult.Error ?? new Error(500, "خطا در افزایش سرمایه"));
        }

        // Update investment account
        //var traceIdStr = increaseResult.Value.TraceId.ToString();
        //investmentAccount.RegisterIncreaseDirect(
        //    amount: GiftCardAmountRial,
        //    traceId: traceIdStr,
        //    receiptDate: receiptDate,
        //    receiptNumber: receiptNumber,
        //    description: description);

        //await _investmentPlanReadRepository.UpdateAsync(investmentAccount, ct);

        _logger.LogInformation(
            "افزایش سرمایه با موفقیت انجام شد. WealthPolicyId: {WealthPolicyId}, TraceId: {TraceId}, Amount: {Amount}",
            wealthPolicyId, increaseResult.Value.TraceId, GiftCardAmountRial);

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Builds the result with gift card details and price information
    /// </summary>
    private async Task<Result<ReceiveGiftCardResult>> BuildResultAsync(
        Guid giftCardId,
        bool isReceived,
        bool isPending,
        string message,
        CancellationToken ct)
    {
        decimal? gramPrice = null;
        decimal? equivalentGrams = null;
        decimal? amount = null;

        // Calculate price info only if received
        if (isReceived && !isPending)
        {
            amount = GiftCardAmountRial;

            try
            {
                var priceInfo = await _investmentProvider.GetCurrentPriceAsync(GiftCardPlanType, ct);
                gramPrice = priceInfo.CurrentPrice;

                if (gramPrice.HasValue && gramPrice.Value > 0)
                {
                    equivalentGrams = GiftCardAmountRial / gramPrice.Value;
                    equivalentGrams = Math.Floor(equivalentGrams.Value * 100m) / 100m; // Round to 2 decimals
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "خطا در دریافت قیمت طلا برای محاسبه معادل کارت هدیه");
                // Continue without price info - not critical
            }
        }

        var result = new ReceiveGiftCardResult
        {
            GiftCardId = giftCardId,
            Message = message,
            IsReceived = isReceived,
            IsPending = isPending,
            GiftCardAmountRial = amount,
            GramPrice = gramPrice,
            EquivalentGrams = equivalentGrams
        };

        return Result<ReceiveGiftCardResult>.Success(result);
    }
}

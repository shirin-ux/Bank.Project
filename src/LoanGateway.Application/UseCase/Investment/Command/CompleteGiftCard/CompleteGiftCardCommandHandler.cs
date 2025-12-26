using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace LoanService.Application.UseCase.Investment.Command.CompleteGiftCard;

public class CompleteGiftCardCommandHandler(
    IGiftCardRepository giftCardRepository,
    IInvestmentProvider investmentProvider,
    IInvestmentPlanReadRepository investmentPlanReadRepository,
    IUserReadService userReadService,
    ILogger<CompleteGiftCardCommandHandler> logger)
    : IRequestHandler<CompleteGiftCardCommand, Result<CompleteGiftCardResult>>
{
    private readonly IGiftCardRepository _giftCardRepository = giftCardRepository;
    private readonly IInvestmentProvider _investmentProvider = investmentProvider;
    private readonly IInvestmentPlanReadRepository _investmentPlanReadRepository = investmentPlanReadRepository;
    private readonly IUserReadService _userReadService = userReadService;
    private readonly ILogger<CompleteGiftCardCommandHandler> _logger = logger;

    public async Task<Result<CompleteGiftCardResult>> Handle(CompleteGiftCardCommand request, CancellationToken cancellationToken)
    {
        // 1. دریافت GiftCard موجود برای کاربر تا OrderId را بگیریم
        var existingGiftCard = await _giftCardRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existingGiftCard == null || existingGiftCard.OrderId == null)
        {
            _logger.LogWarning("کاربر {UserId} GiftCard یا OrderId ندارد.", request.UserId);
            return Result<CompleteGiftCardResult>.Failure(
                new Error(200, "شما هنوز خرید سرمایه‌گذاری انجام نداده‌اید. برای دریافت کارت هدیه ابتدا باید خرید انجام دهید."));
        }

        var orderId = existingGiftCard.OrderId.Value;

        // 2. استعلام وضعیت Order از کاریزما
        var orderStatus = await _investmentProvider.GetOrderBuyByIdAsync(orderId, cancellationToken);


        if (orderStatus.Status != statusType.FinalStatus || orderStatus.UliStatus != false)
        {
            _logger.LogWarning("Order {OrderId} هنوز در وضعیت نهایی نیست. Status: {Status}, UliStatus: {UliStatus}",
                orderId, orderStatus.Status, orderStatus.UliStatus);
            
            return Result<CompleteGiftCardResult>.Failure(
                new Error(200, "بیمه‌نامه هنوز صادر نشده است. لطفاً بعداً تلاش کنید."));
        }

        // 3. دریافت اطلاعات کاربر
        var user = await _userReadService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<CompleteGiftCardResult>.Failure(
                new Error(404, "کاربر یافت نشد."));
        }

        // 4. بررسی اینکه کاربر واجد شرایط دریافت کارت هدیه است (از جدول GiftCardEligibleUsers)
        var isEligible = await _giftCardRepository.IsUserEligibleForGiftCardAsync(request.UserId, cancellationToken);
        _logger.LogInformation(
            "کاربر {UserId} - بررسی واجد شرایط بودن برای کارت هدیه در CompleteGiftCard. IsEligible: {IsEligible}, GiftStatus: {GiftStatus}, NationalCode: {NationalCode}",
            request.UserId,
            isEligible,
            user.GiftStatus,
            user.NationalCode);
        
        if (!isEligible)
        {
            _logger.LogWarning(
                "کاربر {UserId} واجد شرایط دریافت کارت هدیه نیست در CompleteGiftCard. GiftStatus: {GiftStatus}, NationalCode: {NationalCode}",
                request.UserId,
                user.GiftStatus,
                user.NationalCode);
            
            return Result<CompleteGiftCardResult>.Failure(
                new Error(403, "شما مجاز به دریافت کارت هدیه نیستید."));
        }

        // 5. بررسی Race Condition: چک اینکه آیا کاربر قبلاً کارت دریافت کرده
        var hasReceived = await _giftCardRepository.HasUserReceivedGiftAsync(request.UserId, cancellationToken);
        if (hasReceived)
        {
            return Result<CompleteGiftCardResult>.Failure(
                new Error(200, "شما قبلاً کارت هدیه را دریافت کرده‌اید. امکان دریافت مجدد وجود ندارد."));
        }

        try
        {
            // 6. استفاده از GiftCard موجود یا ایجاد جدید
            Domain.Entities.Investment.GiftCard giftCard;
            
            if (existingGiftCard != null)
            {
                // اگر GiftCard قبلاً وجود دارد، از همان استفاده می‌کنیم
                giftCard = existingGiftCard;
                _logger.LogInformation("استفاده از GiftCard موجود برای کاربر {UserId}. GiftCardId: {GiftCardId}",
                    request.UserId, giftCard.Id);
            }
            else
            {
                // اگر GiftCard وجود ندارد، یک جدید ایجاد می‌کنیم
                giftCard = new Domain.Entities.Investment.GiftCard(request.UserId);
                
                // استفاده از متد Atomic برای جلوگیری از Race Condition
                var created = await _giftCardRepository.TryCreateIfNotExistsAsync(giftCard, cancellationToken);
                if (!created)
                {
                    _logger.LogWarning("کاربر {UserId} سعی کرد مجدداً کارت هدیه دریافت کند (Race Condition). OrderId: {OrderId}",
                        request.UserId, orderId);
                    
                    // اگر ایجاد نشد، دوباره از دیتابیس می‌خوانیم
                    existingGiftCard = await _giftCardRepository.GetByUserIdAsync(request.UserId, cancellationToken);
                    if (existingGiftCard != null)
                    {
                        giftCard = existingGiftCard;
                    }
                    else
                    {
                        return Result<CompleteGiftCardResult>.Failure(
                            new Error(200, "شما قبلاً کارت هدیه را دریافت کرده‌اید. امکان دریافت مجدد وجود ندارد."));
                    }
                }
            }

            // 7. بررسی اینکه آیا InvestmentAccount قبلاً ایجاد شده
            var existingAccount = await _investmentPlanReadRepository.GetByNationalCodeAndPlanAsync(
                user.NationalCode!,
                request.PlanType,
                cancellationToken);

            InvestmentAccount investmentAccount;
            if (existingAccount != null && existingAccount.ProviderPolicyId == request.ProviderPolicyId)
            {
                _logger.LogInformation("InvestmentAccount با PolicyId {PolicyId} قبلاً ایجاد شده است.", request.ProviderPolicyId);
                investmentAccount = existingAccount;
            }
            else
            {
                // 8. ایجاد InvestmentAccount
                investmentAccount = InvestmentAccount.CreateNew(
                    policyId: request.ProviderPolicyId,
                    nationalCode: user.NationalCode,
                    birthDate: user.BirthDate,
                    planCode: request.PlanType,
                    traceId: request.TraceId?.ToString() ?? Guid.NewGuid().ToString());

                await _investmentPlanReadRepository.AddAsync(investmentAccount, cancellationToken);
            }

            // 9. افزایش سرمایه با مبلغ کارت هدیه (1,500,000 تومان = 15,000,000 ریال)
            if (orderStatus.WealthPolicyId.HasValue && orderStatus.WealthPolicyId.Value > 0)
            {
                const decimal giftCardAmountRial = 15_000_000m; // 1,500,000 تومان
                var receiptDate = DateTime.UtcNow;
                var receiptNumber = $"GIFT-{orderId:N}"; // شماره مرجع منحصر به فرد
                var description = "افزایش سرمایه از طریق کارت هدیه";
                
                var increaseResult = await _investmentProvider.IncreaseCapitalDirectAsync(
                    wealthPolicyId: orderStatus.WealthPolicyId.Value,
                    amount: giftCardAmountRial,
                    receiptNumber: receiptNumber,
                    receiptDate: receiptDate,
                    description: description,
                    cancellationToken);

                if (!increaseResult.IsSuccess)
                {
                    _logger.LogError("خطا در افزایش سرمایه از طریق کارت هدیه. WealthPolicyId: {WealthPolicyId}, Error: {Error}",
                        orderStatus.WealthPolicyId.Value, increaseResult.Error?.Message);
                    // اگر افزایش سرمایه با خطا مواجه شد، ادامه می‌دهیم (کارت هدیه قبلاً ایجاد شده)
                }
                else if (increaseResult.Value != null)
                {
                   
                    var traceIdStr = increaseResult.Value.TraceId.ToString();
                    investmentAccount.RegisterIncreaseDirect(
                        amount: giftCardAmountRial,
                        traceId: traceIdStr,
                        receiptDate: receiptDate,
                        receiptNumber: receiptNumber,
                        description: description);

                    await _investmentPlanReadRepository.UpdateAsync(investmentAccount, cancellationToken);

                    _logger.LogInformation("افزایش سرمایه کارت هدیه با موفقیت انجام شد. WealthPolicyId: {WealthPolicyId}, TraceId: {TraceId}, Amount: {Amount}",
                        orderStatus.WealthPolicyId.Value, increaseResult.Value.TraceId, giftCardAmountRial);
                }
            }
            else
            {
                _logger.LogWarning("WealthPolicyId موجود نیست برای OrderId: {OrderId}. افزایش سرمایه انجام نشد.", 
                    orderId);
            }

            _logger.LogInformation("کارت هدیه و حساب سرمایه‌گذاری با موفقیت ایجاد شدند. GiftCardId: {GiftCardId}, AccountId: {AccountId}, OrderId: {OrderId}",
                giftCard.Id, investmentAccount.Id, orderId);

            return Result<CompleteGiftCardResult>.Success(new CompleteGiftCardResult
            {
                GiftCardId = giftCard.Id,
                InvestmentAccountId = investmentAccount.Id,
                ProviderPolicyId = request.ProviderPolicyId,
                Message = "کارت هدیه و حساب سرمایه‌گذاری با موفقیت ایجاد شدند."
            });
        }
        catch (SqlException ex) when (ex.Number == 2627) 
        {
            _logger.LogWarning("تلاش برای دریافت مجدد کارت هدیه. UserId: {UserId}, Error: {Error}",
                request.UserId, ex.Message);
            
            return Result<CompleteGiftCardResult>.Failure(
                new Error(200, "شما قبلاً کارت هدیه را دریافت کرده‌اید. امکان دریافت مجدد وجود ندارد."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در تکمیل دریافت کارت هدیه. UserId: {UserId}",
                request.UserId);
            
            return Result<CompleteGiftCardResult>.Failure(
                new Error(500, "خطا در تکمیل دریافت کارت هدیه. لطفاً دوباره تلاش کنید."));
        }
    }
}




using Common;
using LoanService.Application.Contracts;
using LoanService.Application.Exceptions;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlan;

/// <summary>
/// Handler for buying investment plans - creates policy and links to gift card if applicable
/// </summary>
public sealed class BuyPlanCommandHandler(
    IInvestmentPlanReadRepository investmentPlanReadRepository,
    IUserContext userContext,
    IUserReadService userReadService,
    IInvestmentProvider investmentProvider,
    IGiftCardRepository giftCardRepository,
    ILogger<BuyPlanCommandHandler> logger)
    : IRequestHandler<BuyPlanCommand, Result<BuyPlanResultDto>>
{
    private readonly IInvestmentPlanReadRepository _investmentPlanReadRepository = investmentPlanReadRepository;
    private readonly IUserContext _userContext = userContext;
    private readonly IUserReadService _userReadService = userReadService;
    private readonly IInvestmentProvider _investmentProvider = investmentProvider;
    private readonly IGiftCardRepository _giftCardRepository = giftCardRepository;
    private readonly ILogger<BuyPlanCommandHandler> _logger = logger;

    public async Task<Result<BuyPlanResultDto>> Handle(BuyPlanCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("درخواست خرید طرح سرمایه‌گذاری. UserId: {UserId}, PlanType: {PlanType}",
            _userContext.UserId, cmd.PlanType);

        // 1. Validate authentication
        if (!_userContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده");
        }

        // 2. Validate plan exists
        var plan = await _investmentPlanReadRepository.GetPlanAsync(cmd.PlanType.ToString(), ct);
        if (plan is null)
        {
            _logger.LogWarning("طرح سرمایه‌گذاری یافت نشد. PlanType: {PlanType}", cmd.PlanType);
            throw new NotFoundException("طرح سرمایه‌گذاری مورد نظر یافت نشد.");
        }

        // 3. Get user and validate required data
        var user = await _userReadService.GetUserByIdAsync(_userContext.UserId, ct);
        if (user == null)
        {
            _logger.LogWarning("کاربر یافت نشد. UserId: {UserId}", _userContext.UserId);
            return Result<BuyPlanResultDto>.Failure(new Error(404, "کاربر یافت نشد."));
        }

        var nationalCode = GetNationalCode(user);
        if (string.IsNullOrWhiteSpace(nationalCode))
        {
            _logger.LogWarning("کد ملی کاربر موجود نیست. UserId: {UserId}", _userContext.UserId);
            return Result<BuyPlanResultDto>.Failure(
                new Error(400, "کد ملی شما در سیستم ثبت نشده است. لطفاً ابتدا پروفایل خود را تکمیل کنید."));
        }

        if (string.IsNullOrWhiteSpace(user.PostalCode))
        {
            _logger.LogWarning("کد پستی کاربر موجود نیست. UserId: {UserId}", _userContext.UserId);
            return Result<BuyPlanResultDto>.Failure(
                new Error(400, "برای خرید سرمایه‌گذاری، ابتدا باید کد پستی خود را ثبت کنید."));
        }

        // 4. Create policy and buy from provider (Karizma)
        var policyResult = await _investmentProvider.CreatePolicyAndBuyAsync(
            cmd,
            user.BirthDate,
            user.PostalCode,
            nationalCode,
            ct);

        if (!policyResult.IsSuccess)
        {
            _logger.LogError("خطا در ایجاد بیمه‌نامه از کاریزما. UserId: {UserId}, Error: {Error}",
                _userContext.UserId, policyResult.Error?.Message);
            return Result<BuyPlanResultDto>.Failure(policyResult.Error!);
        }

        var orderResult = policyResult.Value!;

        // 5. Link to gift card if applicable
        if (orderResult.OrderId.HasValue)
        {
            await LinkGiftCardIfExistsAsync(_userContext.UserId, orderResult.OrderId.Value, ct);
        }

        // 6. Build and return result
        var resultDto = new BuyPlanResultDto
        {
            PlanType = orderResult.PlanType,
            AmountRial = orderResult.AmountRial,
            ProviderPolicyId = orderResult.ProviderPolicyId,
            OrderId = orderResult.OrderId,
            TraceId = orderResult.TraceId,
            NationalCode = orderResult.NationalCode,
            BirthDate = orderResult.BirthDate,
            PostalCode = user.PostalCode
        };

        _logger.LogInformation("خرید طرح سرمایه‌گذاری با موفقیت انجام شد. UserId: {UserId}, OrderId: {OrderId}",
            _userContext.UserId, orderResult.OrderId);

        return Result<BuyPlanResultDto>.Success(resultDto);
    }

    /// <summary>
    /// Gets national code from user context or user entity
    /// </summary>
    private string? GetNationalCode(UserInfoDto user)
    {
        if (!string.IsNullOrWhiteSpace(_userContext.NationalCode))
        {
            _logger.LogDebug("کد ملی از JWT token استخراج شد. UserId: {UserId}", _userContext.UserId);
            return _userContext.NationalCode;
        }

        if (!string.IsNullOrWhiteSpace(user.NationalCode))
        {
            _logger.LogDebug("کد ملی از Auth Service استخراج شد. UserId: {UserId}", _userContext.UserId);
            return user.NationalCode;
        }

        return null;
    }

    /// <summary>
    /// Links gift card to order if gift card exists for user
    /// </summary>
    private async Task LinkGiftCardIfExistsAsync(Guid userId, Guid orderId, CancellationToken ct)
    {
        try
        {
            var giftCard = await _giftCardRepository.GetByUserIdAsync(userId, ct);
            if (giftCard == null)
            {
                // No gift card exists, create new one with order (but not pending yet - user hasn't clicked receive)
                var newGiftCard = new Domain.Entities.Investment.GiftCard(userId)
                {
                    OrderId = orderId
                    // IsPending stays false - will be set to true when user clicks receive gift card button
                };

                try
                {
                    await _giftCardRepository.CreateAsync(newGiftCard, ct);
                    _logger.LogInformation(
                        "GiftCard جدید با OrderId {OrderId} برای کاربر {UserId} ایجاد شد (IsPending = true)",
                        orderId, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "خطا در ایجاد GiftCard برای کاربر {UserId}. احتمالاً از جای دیگری ایجاد شده است",
                        userId);

                    // Race condition: try to update existing one
                    giftCard = await _giftCardRepository.GetByUserIdAsync(userId, ct);
                    if (giftCard != null)
                    {
                        await UpdateGiftCardOrderAsync(giftCard, orderId, userId, ct);
                    }
                }
            }
            else
            {
                // Gift card exists, update it
                await UpdateGiftCardOrderAsync(giftCard, orderId, userId, ct);
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail the buy operation if gift card linking fails
            _logger.LogError(ex,
                "خطا در لینک کردن GiftCard به Order. UserId: {UserId}, OrderId: {OrderId}",
                userId, orderId);
        }
    }

    /// <summary>
    /// Updates existing gift card with order ID (without setting IsPending - that happens when user clicks receive)
    /// </summary>
    private async Task UpdateGiftCardOrderAsync(
        Domain.Entities.Investment.GiftCard giftCard,
        Guid orderId,
        Guid userId,
        CancellationToken ct)
    {
        await _giftCardRepository.UpdateOrderIdAsync(userId, orderId, ct);
        // Don't set IsPending here - it will be set to true when user clicks "receive gift card" button
        // IsPending = true means: user clicked receive button and we're processing the gift card

        _logger.LogInformation(
            "OrderId {OrderId} برای کاربر {UserId} در GiftCard به‌روزرسانی شد (IsPending هنوز false - کاربر هنوز دکمه دریافت را نزده)",
            orderId, userId);
    }
}

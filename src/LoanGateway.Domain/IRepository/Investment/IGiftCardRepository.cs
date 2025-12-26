using LoanService.Domain.Entities.Investment;

namespace LoanService.Domain.IRepository.Investment;

public interface IGiftCardRepository
{
    Task<GiftCard?> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<bool> HasUserReceivedGiftAsync(Guid userId, CancellationToken ct);
    Task<GiftCard> CreateAsync(GiftCard giftCard, CancellationToken ct);
    /// <summary>
    /// متد Atomic برای چک و ایجاد کارت هدیه (جلوگیری از Race Condition)
    /// این متد با استفاده از MERGE یا IF NOT EXISTS یک عملیات atomic انجام می‌دهد
    /// </summary>
    Task<bool> TryCreateIfNotExistsAsync(GiftCard giftCard, CancellationToken ct);
    /// <summary>
    /// بررسی می‌کند که آیا کاربر در جدول GiftCardEligibleUsers وجود دارد
    /// </summary>
    Task<bool> IsUserEligibleForGiftCardAsync(Guid userId, CancellationToken ct);
    /// <summary>
    /// به‌روزرسانی OrderId در کارت هدیه (بدون تغییر IsReceived)
    /// </summary>
    Task UpdateOrderIdAsync(Guid userId, Guid orderId, CancellationToken ct);
    
    /// <summary>
    /// به‌روزرسانی کارت هدیه بعد از دریافت موفق از کاریزما
    /// </summary>
    Task UpdateAsReceivedAsync(Guid userId, Guid? orderId, CancellationToken ct);
    
    /// <summary>
    /// به‌روزرسانی وضعیت IsPending کارت هدیه
    /// </summary>
    Task UpdateAsPendingAsync(Guid userId, bool isPending, CancellationToken ct);
}




namespace LoanGateway.Auth.Domain.IRepository;

/// <summary>
/// Repository برای بررسی کاربران مجاز به دریافت کارت هدیه
/// </summary>
public interface IGiftCardEligibleUserRepository
{
    /// <summary>
    /// بررسی می‌کند که آیا کاربر با کد ملی مشخص شده در لیست مجازین است
    /// </summary>
    Task<bool> IsEligibleByNationalCodeAsync(string nationalCode, CancellationToken ct);
    
    /// <summary>
    /// علامت‌گذاری کاربر به عنوان پردازش شده (بعد از تنظیم GiftStatus)
    /// </summary>
    Task MarkAsProcessedAsync(string nationalCode, CancellationToken ct);
}










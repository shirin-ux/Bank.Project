using LoanService.Domain.Entities;

namespace LoanService.Domain.Entities.Investment;

/// <summary>
/// موجودیت کارت هدیه - برای ثبت دریافت کارت هدیه توسط کاربران
/// </summary>
public class GiftCard
{
    public Guid Id { get; set; }
    public Guid UserId { get;  set; }
    public DateTime ReceivedAtUtc { get;  set; }
    public Guid? OrderId { get;  set; }
    public decimal EquivalentGrams  { get;  set; }
    public decimal? GiftCardAmountRial { get;  set; }
    public string? Description { get;  set; }
    public bool IsReceived { get; set; }
    public bool IsPending { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get;  set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; protected set; } = default!;

    private GiftCard() { } // برای EF Core

    public GiftCard(Guid userId)
    {
        UserId = userId;
        ReceivedAtUtc = DateTime.UtcNow;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        IsReceived = false;
        IsPending = false;
        OrderId = null;
    }

    public void MarkAsReceived(Guid orderId)
    {
        IsReceived = true;
        OrderId = orderId;
        ReceivedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}


namespace LoanGateway.Auth.Domain.Entities;

public class GiftCardEligibleUser
{
    public Guid Id { get; set; }
    public string NationalCode { get; set; } = default!;
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}


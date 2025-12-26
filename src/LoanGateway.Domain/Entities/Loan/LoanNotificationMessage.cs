namespace LoanService.Domain.Entities.Loan;

public class LoanNotificationMessage:BaseEntity
{
    public Guid LoanId { get; set; }
    public string EventType { get; set; } = default!; 
    public string? Message { get; set; }
    public string? Mobile { get; set; }
    public decimal? Amount { get; set; }
    public string? ExtraData { get; set; }
}

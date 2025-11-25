namespace LoanService.Domain.Entities.Investment;

public class InvestmentPriceSnapshot : BaseEntity
{
    public string PlanCode { get; private set; } = default!;
    public DateTime TimestampUtc { get; private set; }
    public decimal PricePerGram { get; private set; }
    public decimal? PriceChangePercentDaily { get; private set; }
}
namespace LoanService.Domain.Entities.Investment;
public class InvestmentPlanFaq : BaseEntity
{
    public Guid PlanId { get; set; }
    public string Question { get; set; } = default!;
    public string Answer { get; set; } = default!;
    public int Order { get; set; }
}

using LoanService.Domain.Enum.Investment;

namespace LoanService.Domain.Entities.Investment;

public class InvestmentPlan : BaseEntity
{
    public InvestmentPlanType PlanType { get; set; }
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public bool IsActive { get; set; }
    public string SortOrder { get; set; }
    public decimal MinAmount { get; set; }

}

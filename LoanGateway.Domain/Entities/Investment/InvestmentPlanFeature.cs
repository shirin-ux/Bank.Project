namespace LoanService.Domain.Entities.Investment
{
    
    public class InvestmentPlanFeature :BaseEntity
    {
        public Guid PlanId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int Order { get; set; }
    }
}

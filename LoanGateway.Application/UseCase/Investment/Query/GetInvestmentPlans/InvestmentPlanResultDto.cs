using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.UseCase.Investment.Query.GetInvestmentPlans
{
    public class InvestmentPlanResultDto
    {
  
        public Guid Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string ShortDescription { get; init; } = default!;
        public InvestmentPlanType PlanType { get; init; }
        public decimal? MinAmount { get; init; }
    }
}

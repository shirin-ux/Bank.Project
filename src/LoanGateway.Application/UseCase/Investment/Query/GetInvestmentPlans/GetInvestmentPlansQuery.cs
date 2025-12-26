using Common;
using LoanService.Domain.Enum.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.GetInvestmentPlans
{
    public sealed class GetInvestmentPlansQuery : IRequest<Result<List<InvestmentPlanResultDto>>>
    {
        public InvestmentPlanType PlanType { get; init; } = InvestmentPlanType.Gold;
    }
}

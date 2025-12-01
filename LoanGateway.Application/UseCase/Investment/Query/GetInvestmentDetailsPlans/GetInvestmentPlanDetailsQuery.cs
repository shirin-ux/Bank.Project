using Common;
using LoanService.Domain.Enum.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans
{
    public record GetInvestmentPlanDetailsQuery(
            InvestmentPlanType PlanType,
            InvestmentChartRange Range
        ) : IRequest<Result<InvestmentPlanDetailsResultDto>>;

}

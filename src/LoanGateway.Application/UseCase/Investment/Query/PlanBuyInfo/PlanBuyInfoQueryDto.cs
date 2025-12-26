using Common;
using LoanService.Domain.Enum.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.PlanBuyInfo
{
    public sealed record PlanBuyInfoQueryDto(InvestmentPlanType PlanType) : IRequest<Result<PlanBuyInfoResultDto>>;
}

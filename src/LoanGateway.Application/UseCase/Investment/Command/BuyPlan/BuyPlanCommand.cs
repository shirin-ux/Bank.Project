using Common;
using LoanService.Domain.Enum.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlan;

public sealed record BuyPlanCommand: IRequest<Result<BuyPlanResultDto>>

{
   public InvestmentPlanType PlanType { get; set; }

    //public string NationalCode { get; set; }


}

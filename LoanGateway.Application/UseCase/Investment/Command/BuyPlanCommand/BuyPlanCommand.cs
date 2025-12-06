using Common;
using LoanService.Domain.Enum.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlanCommand;

public sealed record BuyPlanCommand: IRequest<Result<BuyPlanResultDto>>

{
   public InvestmentPlanType PlanType { get; set; }
    public long AmountRial { get; set; }
    public string NationalCode { get; set; }
    public string BirthDate { get; set; }
    public string CallbackUrl { get; set; }
    public bool AcceptTerms { get; set; }
    public string  PaymentUrl { get; set; }

}

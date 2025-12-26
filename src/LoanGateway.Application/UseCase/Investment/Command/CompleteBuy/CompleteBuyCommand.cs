using Common;
using LoanService.Domain.Enum.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.CompleteBuy;

public class CompleteBuyCommand : IRequest<Result<CompleteBuyResult>>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid ProviderPolicyId { get; set; }
    public InvestmentPlanType PlanType { get; set; }
    public long? TraceId { get; set; }
}

public class CompleteBuyResult
{
    public Guid InvestmentAccountId { get; set; }
    public Guid ProviderPolicyId { get; set; }
    public string Message { get; set; } = string.Empty;
}


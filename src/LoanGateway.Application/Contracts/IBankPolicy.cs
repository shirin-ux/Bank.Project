using Common;

namespace LoanService.Application.Contracts;

public interface IBankPolicy<TResponse>
{
    Result<ProviderDecisionResult> Evaluate(TResponse response);
}
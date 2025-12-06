namespace LoanService.Application.Contracts;

public interface IInvestmenJobRunner
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

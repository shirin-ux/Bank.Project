using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Command.GetCustomerCreditBalance;
public sealed class GetCustomerCreditBalanceHandler(IBankProviderFactory factory)
    : IRequestHandler<GetCustomerCreditBalanceCommand, GetCustomerCreditBalanceResultDto>
{
    public readonly IBankProviderFactory _factory = factory;
    public async Task<GetCustomerCreditBalanceResultDto> Handle(GetCustomerCreditBalanceCommand cmd, CancellationToken ct)

    {
        var provider = _factory.GetProvider(cmd.ProviderType);
        return await provider.GetCustomerCreditBalanceAsync(cmd, ct);
    }


}

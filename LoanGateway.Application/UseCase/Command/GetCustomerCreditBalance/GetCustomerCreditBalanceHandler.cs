using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Command.GetCustomerCreditBalance;
public sealed class GetCustomerCreditBalanceHandler(IProviderFactory factory)
    : IRequestHandler<GetCustomerCreditBalanceCommand, GetCustomerCreditBalanceResultDto>
{
    public readonly IProviderFactory _factory = factory;
    public async Task<GetCustomerCreditBalanceResultDto> Handle(GetCustomerCreditBalanceCommand cmd, CancellationToken ct)

    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.GetCustomerCreditBalanceAsync(cmd, ct);
    }


}

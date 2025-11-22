using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;


namespace LoanService.Application.UseCase.Command.GetCustomerBilling;

public sealed class GetCustomerBillingHandler(IProviderFactory factory)
 : IRequestHandler<GetCustomerBillingCommand, GetCustomerBillingResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<GetCustomerBillingResultDto> Handle(GetCustomerBillingCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.GetCustomerBillingAsync(cmd, ct);
    }
}

using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;

public sealed class GetCustomerPurchaseDetailsHandler(IBankProviderFactory factory)
    : IRequestHandler<GetCustomerPurchaseDetailsCommand, GetCustomerPurchaseDetailsResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public async Task<GetCustomerPurchaseDetailsResultDto> Handle(GetCustomerPurchaseDetailsCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider(cmd.ProviderType);
        return await provider.GetCustomerPurchaseDetailsAsync(cmd, ct);
    }
}

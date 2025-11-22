using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;

public sealed class GetCustomerPurchaseDetailsHandler(IProviderFactory factory)
    : IRequestHandler<GetCustomerPurchaseDetailsCommand, GetCustomerPurchaseDetailsResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<GetCustomerPurchaseDetailsResultDto> Handle(GetCustomerPurchaseDetailsCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.GetCustomerPurchaseDetailsAsync(cmd, ct);
    }
}

using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Query.GetInstallments;

public sealed class GetInstallmentsHandler(IBankProviderFactory factory)
    : IRequestHandler<GetInstallmentsQuery, GetInstallmentsResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public async Task<GetInstallmentsResultDto> Handle(GetInstallmentsQuery query, CancellationToken ct)

    {
        var provider = _factory.GetProvider(query.ProviderType);
        return await provider.GetInstallmentsAsync(query.NationalCode, query.ContractNumber, ct);
    }

}

using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Query.GetInstallments;

public sealed class GetInstallmentsHandler(IProviderFactory factory)
    : IRequestHandler<GetInstallmentsQuery, GetInstallmentsResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<GetInstallmentsResultDto> Handle(GetInstallmentsQuery query, CancellationToken ct)

    {
        var provider = _factory.GetProvider<IProvider>(query.ProviderType);
        return await provider.GetInstallmentsAsync(query.NationalCode, query.ContractNumber, ct);
    }

}

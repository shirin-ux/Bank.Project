using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Command.GetCollateralContractFile;

public sealed class GetCollateralContractFileHandler(IBankProviderFactory factory)
    : IRequestHandler<GetCollateralContractFileCommand, GetCollateralContractFileResultDto>
{
    private readonly IBankProviderFactory _factory = factory;
    public async  Task<GetCollateralContractFileResultDto> Handle(GetCollateralContractFileCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider(cmd.ProviderType);
        return await provider.GetCollateralContractFileAsync(cmd, ct);
    }
    
}

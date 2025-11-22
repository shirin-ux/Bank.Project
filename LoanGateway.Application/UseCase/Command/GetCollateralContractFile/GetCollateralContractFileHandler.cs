using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Command.GetCollateralContractFile;

public sealed class GetCollateralContractFileHandler(IProviderFactory factory)
    : IRequestHandler<GetCollateralContractFileCommand, GetCollateralContractFileResultDto>
{
    private readonly IProviderFactory _factory = factory;
    public async  Task<GetCollateralContractFileResultDto> Handle(GetCollateralContractFileCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.GetCollateralContractFileAsync(cmd, ct);
    }
    
}

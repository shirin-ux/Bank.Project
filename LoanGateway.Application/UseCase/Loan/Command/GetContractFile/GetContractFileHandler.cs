using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.GetContractFile;


public sealed class GetContractFileHandler(IProviderFactory factory)
    : IRequestHandler<GetContractFileCommand, GetContractFileResultDto>
{
    private readonly IProviderFactory _factory = factory;
    public async Task<GetContractFileResultDto> Handle(GetContractFileCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.GetContractFileAsync(cmd, ct);
    }
}

using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetContractFile;


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

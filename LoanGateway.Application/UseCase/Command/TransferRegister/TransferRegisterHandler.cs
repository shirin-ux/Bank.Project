using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.TransferRegister;

public sealed class TransferRegisterHandler(IProviderFactory factory)
    : IRequestHandler<TransferRegisterCommand, TransferRegisterResultDto>
{
    private readonly IProviderFactory _factory = factory;
    public async Task<TransferRegisterResultDto> Handle(TransferRegisterCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.RegisterTransferAsync(cmd, ct);
    }
}

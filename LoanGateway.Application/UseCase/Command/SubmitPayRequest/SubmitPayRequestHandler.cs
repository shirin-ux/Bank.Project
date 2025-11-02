using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.SubmitPayRequest;

public sealed class SubmitPayRequestHandler(IBankProviderFactory factory)
    : IRequestHandler<SubmitPayRequestCommand,SubmitPayRequestResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public async Task<SubmitPayRequestResultDto> Handle(SubmitPayRequestCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider(cmd.ProviderType);
        return await provider.SubmitPayRequestAsync(cmd, ct);
    }
}

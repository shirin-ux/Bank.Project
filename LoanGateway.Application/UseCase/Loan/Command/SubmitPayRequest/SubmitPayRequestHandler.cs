using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Loan.Command.SubmitPayRequest;

public sealed class SubmitPayRequestHandler(IProviderFactory factory)
    : IRequestHandler<SubmitPayRequestCommand,SubmitPayRequestResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<SubmitPayRequestResultDto> Handle(SubmitPayRequestCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.SubmitPayRequestAsync(cmd, ct);
    }
}

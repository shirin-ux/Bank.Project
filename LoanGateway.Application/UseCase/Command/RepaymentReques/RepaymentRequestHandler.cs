using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.RepaymentReques;

public sealed class RepaymentRequestHandler(IBankProviderFactory factory)
    : IRequestHandler<RepaymentRequestCommand, RepaymentRequestResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public  async Task<RepaymentRequestResultDto> Handle(RepaymentRequestCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider(cmd.ProviderType);
        return await provider.RepaymentRequestAsync(cmd, ct);
    }
}

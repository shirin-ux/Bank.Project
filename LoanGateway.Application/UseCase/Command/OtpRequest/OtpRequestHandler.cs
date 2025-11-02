using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.OtpRequest;

public sealed class OtpRequestHandler(IBankProviderFactory factory)
: IRequestHandler<OtpRequestCommand, OtpRequestResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public async Task<OtpRequestResultDto> Handle(OtpRequestCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider(cmd.ProviderType);
        return await provider.RequestOtpAsync(cmd, ct);
    }
}

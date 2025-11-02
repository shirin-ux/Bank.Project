using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.ReturnTransferReport;

public sealed class ReturnTransferReportHandler(IBankProviderFactory factory)
    : IRequestHandler<ReturnTransferReportCommand,ReturnTransferReportResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public async Task<ReturnTransferReportResultDto> Handle(ReturnTransferReportCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider(cmd.ProviderType);
        return await provider.GetReturnTransferReportAsync(cmd, ct);
    }
}

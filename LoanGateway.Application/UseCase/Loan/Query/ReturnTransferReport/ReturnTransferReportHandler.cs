using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Query.ReturnTransferReport;

public sealed class ReturnTransferReportHandler(IProviderFactory factory)
    : IRequestHandler<ReturnTransferReportCommand, ReturnTransferReportResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<ReturnTransferReportResultDto> Handle(ReturnTransferReportCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.GetReturnTransferReportAsync(cmd, ct);
    }
}

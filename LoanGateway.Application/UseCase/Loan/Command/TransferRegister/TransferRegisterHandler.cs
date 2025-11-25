using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.TransferRegister;

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

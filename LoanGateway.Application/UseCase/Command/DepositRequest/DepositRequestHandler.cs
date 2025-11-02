
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Command.DepositRequest;

public sealed class DepositRequestHandler(IBankProviderFactory factory)
    : IRequestHandler<DepositRequestCommand, DepositRequestResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public async Task<DepositRequestResultDto> Handle(DepositRequestCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider(cmd.ProviderType);
        return await provider.DepositRequestAsync(cmd, ct);
    }
       
}

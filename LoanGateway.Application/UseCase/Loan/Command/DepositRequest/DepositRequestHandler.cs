
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.DepositRequest;

public sealed class DepositRequestHandler(IProviderFactory factory)
    : IRequestHandler<DepositRequestCommand, DepositRequestResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<DepositRequestResultDto> Handle(DepositRequestCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.DepositRequestAsync(cmd, ct);
    }
       
}

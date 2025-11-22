using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Command.OtpRequest;

public sealed class OtpRequestHandler(IProviderFactory factory)
: IRequestHandler<OtpRequestCommand, OtpRequestResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<OtpRequestResultDto> Handle(OtpRequestCommand cmd, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(cmd.ProviderType);
        return await provider.RequestOtpAsync(cmd, ct);
    }
}

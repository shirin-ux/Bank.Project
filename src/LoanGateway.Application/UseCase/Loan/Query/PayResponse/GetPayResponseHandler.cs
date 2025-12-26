using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Query.PayResponse;
public sealed class GetPayResponseHandler(IProviderFactory factory)
    : IRequestHandler<GetPayResponseQuery, GetPayResponseResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<GetPayResponseResultDto> Handle(GetPayResponseQuery q, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(q.ProviderType);
        return await provider.GetPayResponseAsync(q.PayRequestId, ct);
    }
}

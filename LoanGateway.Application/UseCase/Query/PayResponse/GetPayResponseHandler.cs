using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.PayResponse;
public sealed class GetPayResponseHandler(IBankProviderFactory factory)
    : IRequestHandler<GetPayResponseQuery, GetPayResponseResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public  async Task<GetPayResponseResultDto> Handle(GetPayResponseQuery q, CancellationToken ct)
    {
        var provider = _factory.GetProvider(q.ProviderType);
        return await provider.GetPayResponseAsync(q.PayRequestId, ct);
    }
}

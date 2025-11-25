using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Loan.Query.TransferInquiry;

public sealed class TransferInquiryHandler(IProviderFactory factory)
    : IRequestHandler<TransferInquiryQuery, TransferInquiryResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<TransferInquiryResultDto> Handle(TransferInquiryQuery q, CancellationToken ct)

    {
        var provider = _factory.GetProvider<IProvider>(q.ProviderType);
        return await provider.TransferInquiryAsync(q, ct);
    }
}
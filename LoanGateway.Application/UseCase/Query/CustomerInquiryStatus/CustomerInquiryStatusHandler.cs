using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.CustomerInquiryStatus;

public sealed class CustomerInquiryStatusHandler(IBankProviderFactory factory)
: IRequestHandler<GetCustomerInquiryStatusQuery, CustomerInquiryStatusResultDto>
{
    private readonly IBankProviderFactory _factory = factory;

    public async Task<CustomerInquiryStatusResultDto> Handle(GetCustomerInquiryStatusQuery query, CancellationToken ct)
    {
        var provider = _factory.GetProvider(query.ProviderType);
        return await provider.GetCustomerInquiryStatusAsync(query.RequestId, ct);
    }

} 
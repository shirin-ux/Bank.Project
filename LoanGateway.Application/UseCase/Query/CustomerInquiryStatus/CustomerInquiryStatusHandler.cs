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

public sealed class CustomerInquiryStatusHandler(IProviderFactory factory)
: IRequestHandler<GetCustomerInquiryStatusQuery, CustomerInquiryStatusResultDto>
{
    private readonly IProviderFactory _factory = factory;

    public async Task<CustomerInquiryStatusResultDto> Handle(GetCustomerInquiryStatusQuery query, CancellationToken ct)
    {
        var provider = _factory.GetProvider<IProvider>(query.ProviderType);
        return await provider.GetCustomerInquiryStatusAsync(query.RequestId, ct);
    }

} 
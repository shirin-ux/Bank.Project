using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Loan.Query.CustomerInquiryStatus;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.CustomerInquiryStatus;

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
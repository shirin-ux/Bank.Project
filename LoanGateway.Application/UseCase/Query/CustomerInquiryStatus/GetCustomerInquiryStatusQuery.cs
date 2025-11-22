using LoanService.Domain.Enum;
using MediatR;


namespace LoanService.Application.UseCase.Query.CustomerInquiryStatus;

public sealed class GetCustomerInquiryStatusQuery
    : IRequest<CustomerInquiryStatusResultDto>
{
    public string RequestId { get; set; } = default!;
    public Guid LoanId { get; set; } = default!;
    public ProviderType ProviderType { get; set; }

}

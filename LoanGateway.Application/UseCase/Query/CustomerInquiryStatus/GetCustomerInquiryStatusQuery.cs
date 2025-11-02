using Common;
using LoanService.Domain.Enum;
using MediatR;


namespace LoanService.Application.UseCase.Query.CustomerInquiryStatus;

public sealed class GetCustomerInquiryStatusQuery 
    : IRequest<CustomerInquiryStatusResultDto>
{
    public string RequestId { get; set; } = default!;
    public BankProviderType ProviderType { get; set; }
}

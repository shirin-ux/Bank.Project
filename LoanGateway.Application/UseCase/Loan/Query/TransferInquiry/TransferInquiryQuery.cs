using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Query.TransferInquiry;

public sealed record TransferInquiryQuery
    : IRequest<TransferInquiryResultDto>
{
    public string RegisterCode { get; set; }
    public ProviderType ProviderType { get; set; }
}
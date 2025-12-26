using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.CustomerInquiry;

public sealed record CustomerInquiryCommand : IRequest<CustomerInquiryResultDto>
{
    public ProviderType ProviderType { get; set; }
    public string NationalCode { get; init; } = default!;
   // public short? ConfigType { get; init; }
    public string? BirthDate { get; init; }
    public string? MobileNo { get; init; }
    public string? PostalCode { get; init; }
    public decimal? RequestAmount { get; init; }
    public decimal? ApprovalCode { get; init; }
    public decimal? CbTrackingCode { get; init; }

}

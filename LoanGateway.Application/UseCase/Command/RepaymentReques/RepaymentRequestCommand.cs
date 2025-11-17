using LoanService.Domain.Enum.Loan;
using MediatR;

namespace LoanService.Application.UseCase.Command.RepaymentReques;
public sealed record RepaymentRequestCommand : IRequest<RepaymentRequestResultDto>
{
    public string AccountNo { get; init; }
    public string ContractNo { get; init; }
    public string NationalCode { get; init; } = default!;
    public decimal RepaymentAmount { get; init; }
    public BankProviderType ProviderType { get; set; }
    public int? OtpCode { get; init; }
}
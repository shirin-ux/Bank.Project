using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Command.RepaymentReques;
public sealed record RepaymentRequestCommand : IRequest<RepaymentRequestResultDto>
{
    public decimal AccountNo { get; init; }
    public decimal ContractNo { get; init; }
    public string NationalCode { get; init; } = default!;
    public decimal RepaymentAmount { get; init; }
    public BankProviderType ProviderType { get; set; }
    public int? OtpCode { get; init; }
}
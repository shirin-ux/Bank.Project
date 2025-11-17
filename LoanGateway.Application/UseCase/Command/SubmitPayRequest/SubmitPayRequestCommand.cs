using LoanService.Domain.Enum.Loan;
using MediatR;


namespace LoanService.Application.UseCase.Command.SubmitPayRequest;
public sealed record SubmitPayRequestCommand : IRequest<SubmitPayRequestResultDto>
{
    public decimal ContractNumber { get; init; }
    public decimal? RequestAmount { get; init; }
    public string? ContractPath { get; init; }
    public BankProviderType ProviderType { get; set; }
}
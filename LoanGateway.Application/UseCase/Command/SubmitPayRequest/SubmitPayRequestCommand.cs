using Common;
using LoanService.Domain.Enum;
using MediatR;


namespace LoanService.Application.UseCase.Command.SubmitPayRequest;
public sealed record SubmitPayRequestCommand : IRequest<SubmitPayRequestResultDto>
{
    public decimal ContractNumber { get; init; }          // DECIMAL(20) - required
    public decimal? RequestAmount { get; init; }          // DECIMAL(20) - optional
    public byte[]? ContractFile { get; init; }  // 64BASE - required
    public Guid LoanRequestId { get; init; }
    public BankProviderType ProviderType { get; set; }
}
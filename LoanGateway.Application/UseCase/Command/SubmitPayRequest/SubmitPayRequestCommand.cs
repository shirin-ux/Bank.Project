using LoanService.Domain.Enum;
using MediatR;


namespace LoanService.Application.UseCase.Command.SubmitPayRequest;
public sealed record SubmitPayRequestCommand : IRequest<SubmitPayRequestResultDto>
{
    public decimal ContractNumber { get; init; }
    public decimal? RequestAmount { get; init; }
    public string? ContractPath { get; init; }
    public ProviderType ProviderType { get; set; }
}
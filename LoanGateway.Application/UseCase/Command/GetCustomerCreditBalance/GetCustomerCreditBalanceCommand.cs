using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Command.GetCustomerCreditBalance;

public sealed record GetCustomerCreditBalanceCommand : IRequest<GetCustomerCreditBalanceResultDto>
{
    public ProviderType ProviderType { get; set; }
    public string ContractNumber { get; set; }
    public string NationalCode { get; set; }
}
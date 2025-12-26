using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.GetCustomerBilling;

public sealed record GetCustomerBillingCommand(
    short BillingNumber,
    string ContractNumber,
    string NationalCode,
   ProviderType ProviderType
) : IRequest<GetCustomerBillingResultDto>;

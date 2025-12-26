using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.GetCustomerPurchaseDetails;

public sealed record GetCustomerPurchaseDetailsCommand(
    string ContractNumber,
    ProviderType ProviderType,
    string NationalCode,
    string FromDate, // DD/MM/YYYY
    string ToDate    // DD/MM/YYYY
) : IRequest<GetCustomerPurchaseDetailsResultDto>;

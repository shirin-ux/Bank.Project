using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Query.ReturnTransferReport;

public sealed record ReturnTransferReportCommand(
    ProviderType ProviderType,
    int ReturnDate,          // YYYYMMDD (INTEGER) - اجباری
    decimal FromId           // DECIMAL(18) - اجباری (اولین بار صفر)
) : IRequest<ReturnTransferReportResultDto>;

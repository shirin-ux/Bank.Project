using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.OtpRequest;
using LoanService.Domain.Enum.Loan;

namespace LoanService.Application.Contracts;

public interface ILoanOrchestratorJobRunner
{
    Task EnqueuePayResponseInquiryAsync(Guid loanId, string payRequestId,  TimeSpan delay, CancellationToken ct);
    Task EnqueueDecisionPollingAsync(Guid loanRequestId, CancellationToken ct);
    Task EnqueueRemittanceInquiryAsync(Guid loanRequestId, string remittanceId, CancellationToken ct);
    Task EnqueueRetryGetInstallment(Guid loanId, TimeSpan delay, CancellationToken ct);
    Task EnqueueRetryCreditBalance(Guid loanId, TimeSpan delay, CancellationToken ct);
    Task EnqueueDepositRetryAsync(Guid loanId, TimeSpan delay, CancellationToken ct, DepositRequestCommand cmd);
    Task EnqueueInquiryResultRetryAsync(Guid loanId, BankProviderType providerType, TimeSpan delay, CancellationToken ct);
    Task EnqueueOtpRequestRetryAsync(Guid loanId, OtpRequestCommand cmd, TimeSpan delay, CancellationToken ct);
}

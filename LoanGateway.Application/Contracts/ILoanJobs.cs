using LoanService.Application.UseCase.Command.DepositRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts;

public interface ILoanJobs
{
    Task EnqueuePayResponseInquiryAsync(Guid loanId, string payRequestId, TimeSpan delay, CancellationToken ct);
    Task EnqueueDecisionPollingAsync(Guid loanRequestId, CancellationToken ct);
    Task EnqueueRemittanceInquiryAsync(Guid loanRequestId, string remittanceId, CancellationToken ct);
    Task EnqueueRetryGetInstallment(Guid loanId, TimeSpan delay, CancellationToken ct);
    Task EnqueueRetryCreditBalance(Guid loanId, TimeSpan delay, CancellationToken ct);
    Task EnqueueDepositRetryAsync(Guid loanId, TimeSpan delay, CancellationToken ct, DepositRequestCommand cmd);
}

using Hangfire;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.StartLoanRequestDto;
using LoanService.Domain.Entities;
using LoanService.Domain.IRepository;
using Microsoft.Extensions.DependencyInjection;


namespace LoanService.Infrastructure.Jobs;
public sealed class HangfireLoanJobs : ILoanJobs
{

    private readonly IBackgroundJobClient _bg;
    private readonly LoanJobRunner _runner;

    public HangfireLoanJobs(IBackgroundJobClient bg, LoanJobRunner runner)
    {
        _bg = bg;
        _runner = runner;
   
    }

    public Task EnqueueDecisionPollingAsync(Guid loanRequestId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task EnqueuePayResponseInquiryAsync(Guid loanId, string payRequestId, TimeSpan delay, CancellationToken ct)
    {
        _bg.Schedule<LoanJobRunner>(
            r => r.RunPayResponseInquiryAsync(loanId, payRequestId, CancellationToken.None),
            delay);

        return Task.CompletedTask;
    }

    public Task EnqueueRetryGetInstallment(Guid loanId, TimeSpan delay, CancellationToken ct)
    {
        _bg.Schedule<LoanJobRunner>(r => r.RetryGetInstallments(loanId, ct), delay);
        return Task.CompletedTask;
    }
    public Task EnqueueRetryCreditBalance(Guid loanId, TimeSpan delay, CancellationToken ct)
    {
        _bg.Schedule<LoanJobRunner>(r => r.RetryCreditBalance(loanId, ct),delay);
        return Task.CompletedTask;
    }
    public Task EnqueueDepositRetryAsync(Guid loanId, TimeSpan delay, CancellationToken ct, DepositRequestCommand cmd)
    {
        _bg.Schedule<LoanJobRunner>(r => r.RetryDeposit(loanId, ct,cmd), delay);
        return Task.CompletedTask;
    }
    public Task EnqueueRemittanceInquiryAsync(Guid loanRequestId, string remittanceId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }


}



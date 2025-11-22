using Hangfire;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.OtpRequest;
using LoanService.Domain.Enum;
using Microsoft.Extensions.Logging;


namespace LoanService.Infrastructure.Jobs;
public sealed class LoanOrchestratorJobs : ILoanOrchestratorJobRunner
{

    private readonly IBackgroundJobClient _bg;
    private readonly LoanJobRunner _runner;
    private readonly ILogger<LoanOrchestratorJobs> _logger;
    public LoanOrchestratorJobs(IBackgroundJobClient bg, LoanJobRunner runner, ILogger<LoanOrchestratorJobs> logger)
    {
        _bg = bg;
        _runner = runner;
        _logger = logger;
    }

    public Task EnqueueDecisionPollingAsync(Guid loanRequestId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task EnqueuePayResponseInquiryAsync(Guid loanId, string payRequestId, TimeSpan delay, CancellationToken ct)
    {

        _bg.Schedule<LoanJobRunner>(
        r => r.RunPayResponseInquiryAsync(loanId, payRequestId, 1, CancellationToken.None),
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
        _bg.Schedule<LoanJobRunner>(r => r.RetryCreditBalance(loanId, ct), delay);
        return Task.CompletedTask;
    }
    public Task EnqueueDepositRetryAsync(Guid loanId, TimeSpan delay, CancellationToken ct, DepositRequestCommand cmd)
    {
        _bg.Schedule<LoanJobRunner>(r => r.RetryDeposit(loanId, ct, cmd), delay);
        return Task.CompletedTask;
    }
    public Task EnqueueRemittanceInquiryAsync(Guid loanRequestId, string remittanceId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task EnqueueInquiryResultRetryAsync(Guid loanId, ProviderType providerType, TimeSpan delay, CancellationToken ct)
    {
        _bg.Schedule<LoanJobRunner>(runner => runner.RunInquiryResultAsync(loanId, providerType, ct), delay);

        _logger.LogInformation("Scheduled inquiry-result retry for Loan {LoanId} after {Delay} seconds", loanId, delay.TotalSeconds);

        return Task.CompletedTask;
    }

    public Task EnqueueOtpRequestRetryAsync(Guid loanId, OtpRequestCommand cmd, TimeSpan delay, CancellationToken ct)
    {
        _bg.Schedule<LoanJobRunner>(runner => runner.RunSendOtp(loanId, cmd), delay);

        _logger.LogInformation("Scheduled SendOtpAsync retry for Loan {LoanId} after {Delay} seconds", loanId, delay.TotalSeconds);

        return Task.CompletedTask;
    }

    public Task EnqueueTransferInquiryAsync(Guid loanId, TimeSpan delay, CancellationToken ct)
    {
        _bg.Schedule<LoanJobRunner>(runner => runner.RunTransferRegister(loanId), delay);

        _logger.LogInformation("Scheduled SendOtpAsync retry for Loan {LoanId} after {Delay} seconds", loanId, delay.TotalSeconds);

        return Task.CompletedTask;
    }
}



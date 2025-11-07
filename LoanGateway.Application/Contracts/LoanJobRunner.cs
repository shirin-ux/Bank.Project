using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.StartLoanRequestDto;
using LoanService.Domain.Entities;
using LoanService.Domain.Enum;
using LoanService.Domain.IRepository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoanService.Application.Contracts;

public class LoanJobRunner(
                   IMediator mediator,
                   ILoanRequestRepository repo,
                   ILogger<LoanJobRunner> logger,
                   IServiceProvider serviceProvider
                         )
{
    private readonly IMediator _mediator = mediator;
    private readonly ILoanRequestRepository _repo = repo;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<LoanJobRunner> _logger = logger;
    public async Task RunPayResponseInquiryAsync(Guid loanId, string payRequestId, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ILoanRequestRepository>();
        var service = scope.ServiceProvider.GetRequiredService<LoanRequestOrchestrator>();

        var loan = await _repo.GetByIdAsync(loanId, ct);
        if (loan is null) return;
        _logger.LogInformation("Running scheduled GetInstallment for Loan {LoanId}", loanId);

    }


    public async Task RetryGetInstallments(Guid loanId, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ILoanRequestRepository>();

        var service = scope.ServiceProvider.GetRequiredService<LoanRequestOrchestrator>();

        var loan = await repo.GetByIdAsync(loanId, CancellationToken.None);
        if (loan is null || loan.State != LoanRequestState.Failed)
            return;
        _logger.LogInformation("Running scheduled GetInstallment for Loan {LoanId}", loanId);

        var result = await service.GetInstallmentsAsync(loanId, CancellationToken.None);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Scheduled GetInstallment for Loan {LoanId} failed: {Msg}", loanId, result.Error?.Message);
        }
    }

    public async Task RetryCreditBalance(Guid loanId, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<LoanRequestOrchestrator>();

        _logger.LogInformation("Running scheduled CreditBalance for Loan {LoanId}", loanId);

        var result = await service.GetCreditBalanceAsync(loanId, ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Scheduled CreditBalance for Loan {LoanId} failed: {Msg}", loanId, result.Error?.Message);
        }

    }
    public async Task RetryDeposit(Guid loanId, CancellationToken ct, DepositRequestCommand cmd)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<LoanRequestOrchestrator>();

        _logger.LogInformation("Running scheduled Deposit for Loan {LoanId}", loanId);

        var result = await service.DepositRequestAsync(loanId, cmd, ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Scheduled Deposit for Loan {LoanId} failed: {Msg}", loanId, result.Error?.Message);
        }
    }
    public async Task RunInquiryResultAsync(Guid loanId, BankProviderType providerType, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<LoanRequestOrchestrator>();

        _logger.LogInformation("Running scheduled inquiry-result for Loan {LoanId}", loanId);

        var result = await service.GetInquiryResultAsync(loanId, providerType, ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Scheduled inquiry-result for Loan {LoanId} failed: {Msg}", loanId, result.Error?.Message);
        }
    }
}


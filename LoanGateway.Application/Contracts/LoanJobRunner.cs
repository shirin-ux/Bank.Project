using Bank.Mellat.Provider.Dtos;
using LoanService.Application.PloicyProvider.Mellat;
using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.StartLoanRequestDto;
using LoanService.Application.UseCase.Query.PayResponse;
using LoanService.Domain.Entities;
using LoanService.Domain.IRepository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts;

public class LoanJobRunner(

         IMediator mediator,
        ILoanRequestRepository repo,
       // IMellatGenericPolicy<GetPayResponseResultDto> payRespPolicy,
         IServiceProvider serviceProvider
    )
{
    private readonly IMediator _mediator= mediator;
    private readonly ILoanRequestRepository _repo=repo;
    private readonly IServiceProvider _serviceProvider= serviceProvider;
   // private readonly IMellatGenericPolicy<GetPayResponseResultDto> _payRespPolicy= payRespPolicy;
    public async Task RunPayResponseInquiryAsync(Guid loanId, string payRequestId, CancellationToken ct = default)
    {
        var loan = await _repo.GetByIdAsync(loanId, ct);
        if (loan is null) return;

        //از بانک استعلام میگیریم
        var res = await _mediator.Send(new GetPayResponseQuery
        {
            PayRequestId= payRequestId
        }, ct);

        // 2️⃣ تصمیم‌گیری با پالیسی بانک ملت
        //var decision = payRespPolicy.Evaluate(res);
        //if (!decision.Value.Result.IsSuccess)
        //{
        //    // خطا موقتی → دوباره در نوبت گذاشته می‌شود
        //    return;
        //}

        //var d = decision.Value!;

        //// 3️⃣ به‌روزرسانی وضعیت دامنه
        //loan.TransitionTo(d.NextState, d.ReasonCode, d.UiMessage);

        //if (d.NextState == LoanRequestState.Approved)
        //{
        //    loan.MarkApproved(d.ReasonCode, d.UiMessage);
        //}
        //else if (d.NextState == LoanRequestState.Failed)
        //{
        //    loan.MarkTemporaryFailure(d.ReasonCode, d.UiMessage);
        //}

        //await _repo.UpdateAsync(loan, ct);
    }


    public async Task RetryGetInstallments(Guid loanId, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ILoanRequestRepository>();
        var service = scope.ServiceProvider.GetRequiredService<LoanRequestOrchestrator>();

        var loan = await repo.GetByIdAsync(loanId, CancellationToken.None);
        if (loan is null || loan.State != LoanRequestState.Failed)
            return;

        await service.GetInstallmentsAsync(loanId, CancellationToken.None);
    }

    public async Task RetryCreditBalance(Guid loanId,CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<LoanRequestOrchestrator>();
            await service.GetCreditBalanceAsync(loanId,ct);

    }
    public async Task RetryDeposit(Guid loanId, CancellationToken ct, DepositRequestCommand cmd)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<LoanRequestOrchestrator>();
        await service.DepositRequestAsync(loanId, cmd, ct);
    }
}


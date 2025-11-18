using Common;
using FluentValidation;
using LoanService.Application.Contracts;
using LoanService.Application.Exceptions;
using LoanService.Application.UseCase.Command.CustomerInquiry;
using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.GetCollateralContractFile;
using LoanService.Application.UseCase.Command.GetContractFile;
using LoanService.Application.UseCase.Command.GetCustomerBilling;
using LoanService.Application.UseCase.Command.GetCustomerCreditBalance;
using LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;
using LoanService.Application.UseCase.Command.OtpRequest;
using LoanService.Application.UseCase.Command.RepaymentReques;
using LoanService.Application.UseCase.Command.SubmitPayRequest;
using LoanService.Application.UseCase.Command.TransferRegister;
using LoanService.Application.UseCase.Query.CustomerInquiryStatus;
using LoanService.Application.UseCase.Query.GetInstallments;
using LoanService.Application.UseCase.Query.PayResponse;
using LoanService.Application.UseCase.Query.TransferInquiry;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.Enum.Loan;
using LoanService.Domain.IRepository;
using MapsterMapper;
using MediatR;
using static LoanService.Application.UseCase.Command.OtpRequest.OtpRequestCommand;



namespace LoanService.Application.UseCase.Command.StartLoanRequestDto;

public sealed class LoanRequestOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILoanRequestRepository _repo;
    private readonly ILoanOrchestratorJobRunner _jobs;
    private readonly IBankPolicyFactory _bankPolicyFactory;
    private readonly IMapper _mapper;
    private readonly IContractFileStorage _contractFileStorage;
    private readonly ILoanNotificationBus _loanNotification;
    public LoanRequestOrchestrator(
        IMediator mediator,
        ILoanRequestRepository repo,
        ILoanOrchestratorJobRunner jobs,
        IMapper mapper,
        IBankPolicyFactory bankPolicyFactory,
        IContractFileStorage contractFileStorage,
        ILoanNotificationBus loanNotification)

    {
        _mapper = mapper;
        _mediator = mediator;
        _repo = repo;
        _jobs = jobs;
        _bankPolicyFactory = bankPolicyFactory;
        _contractFileStorage = contractFileStorage;
        _loanNotification = loanNotification;
    }


    public async Task<Result<CustomerInquiryResultDto>> StartInquiryAsync(CustomerInquiryCommand cmd, LoanRequest loan, CancellationToken ct)
    {
        try
        {
            var bankRes = await _mediator.Send(cmd, ct);

            await _repo.InsertAsync(loan, ct);

            var providerType = cmd.ProviderType;

            var policy = _bankPolicyFactory.CreatePolicy<CustomerInquiryResultDto>(providerType, "CustomerInquiry");

            var decisionResult = policy.Evaluate(bankRes);

            if (!decisionResult.IsSuccess)
                return await FailAndReturn<CustomerInquiryResultDto>(loan, decisionResult.Error!, ct);

            var decision = decisionResult.Value!;

            decimal? amount = cmd.RequestAmount;

            bool requiresCollateral = false;
            CollateralType collateralType = CollateralType.Unknown;



            loan.SetRequest(cmd.RequestAmount, requiresCollateral, collateralType);
            loan.SetInquiryRequestId(bankRes.RequestId);
            if (decision.NextState is not null)
            {
                loan.TransitionTo(decision.NextState.Value, decision.ReasonCode, decision.UiMessage);
            }
            await _repo.UpdateAsync(loan, ct);

            return Result<CustomerInquiryResultDto>.Success(
                new CustomerInquiryResultDto
                {
                    RequestId = bankRes.RequestId,
                    Message = bankRes.Message,
                    MessageCode = bankRes.MessageCode,
                    State = bankRes.State
                });
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<CustomerInquiryResultDto>.Failure(error);

        }
        catch (TransientException ex)
        {
            loan.TransitionTo(LoanRequestState.Failed, 10101, "سرویس بانک در دسترس نیست. بعداً تلاش کنید.");
            await _repo.UpdateAsync(loan, ct);

            return Result<CustomerInquiryResultDto>.Failure(new Error(10101, ex.Message));
        }
        catch (System.Exception ex)
        {
            loan.TransitionTo(LoanRequestState.Failed, -1, "خطای داخلی رخ داد.");
            await _repo.UpdateAsync(loan, ct);

            return Result<CustomerInquiryResultDto>.Failure(new Error(-1, "خطای داخلی رخ داد."));
        }
    }


    public async Task<Result<CustomerInquiryStatusResultDto>> GetInquiryResultAsync(Guid loanId, BankProviderType ProviderType, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);

        if (loan is null)
            return Result<CustomerInquiryStatusResultDto>.Failure(new Error(-1, "درخواست وام یافت نشد."));

        if (string.IsNullOrWhiteSpace(loan.InqueryRequest.RequestId.ToString()))
            return Result<CustomerInquiryStatusResultDto>.Failure(new Error(-1, "برای این وام هنوز requestId استعلام ثبت نشده است."));


        var bankRes = await _mediator.Send(new GetCustomerInquiryStatusQuery
        {
            RequestId = loan.InqueryRequest.RequestId,
            ProviderType = ProviderType
        }, ct);

        var providerType = ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<CustomerInquiryStatusResultDto>(providerType, "CustomerInquiryResult");

        var decisionResult = policy.Evaluate(bankRes);
        if (!decisionResult.IsSuccess)
        {

            loan.TransitionTo(LoanRequestState.Failed, decisionResult.Error!.Code, decisionResult.Error.Message);

            await _repo.UpdateAsync(loan, ct);

            return Result<CustomerInquiryStatusResultDto>.Failure(decisionResult.Error);
        }
        var decision = decisionResult.Value!;

        if (decision.Retryable == true)
        {

            loan.TransitionTo(
                decision.NextState ?? LoanRequestState.KYCChecked,
                decision.ReasonCode,
                decision.UiMessage);

            await _repo.UpdateAsync(loan, ct);


            await _jobs.EnqueueInquiryResultRetryAsync(loan.Id, providerType, TimeSpan.FromSeconds(30), ct);

            return Result<CustomerInquiryStatusResultDto>.Failure(
                new Error(decision.ReasonCode, decision.UiMessage ?? "نتیجه استعلام هنوز آماده نیست."));
        }

        loan.SetInquiryDecision(bankRes.Allowed, bankRes.MaxApprovedAmount, bankRes.StatusList.Select(x => new InquiryInfo.StatusItem
        {
            ResponseCode = x.ResponseCode,
            ResponseStatus = x.ResponseStatus
        }).ToList(), bankRes.RequestExpireDate);

        if (decision.NextState is not null)
        {
            loan.TransitionTo(
                decision.NextState.Value,
                decision.ReasonCode,
                decision.UiMessage);
        }
        else
        {
            loan.TransitionTo(
                bankRes.Allowed ? LoanRequestState.Eligible : LoanRequestState.Ineligible,
                decision.ReasonCode,
                decision.UiMessage);
        }

        await _repo.UpdateAsync(loan, ct);

        return Result<CustomerInquiryStatusResultDto>.Success(new CustomerInquiryStatusResultDto
        {
            RequestId = bankRes.RequestId,
            Allowed = bankRes.Allowed,
            MaxApprovedAmount = bankRes.MaxApprovedAmount,
            RequestExpireDate = bankRes.RequestExpireDate,
            Message = decision.UiMessage,
            State = loan.State.ToString(),
            StatusList = bankRes.StatusList.Select(x => new CustomerInquiryStatusResultDto.StatusItemDto(x.ResponseStatus, x.ResponseCode)).ToList(),
            NextActions = GetNextActions(loan)
        });
    }


    public async Task<Result<GetContractFileResultDto>> GetContractFileNoCollateralAsync(Guid loanId, GetContractFileCommand cmd, CancellationToken ct)
    {
        try
        {
            var loan = await RequireAsync(loanId, ct);

            if (loan is null)
                return Result<GetContractFileResultDto>.Failure(new Error(-1, "درخواست وام یافت نشد."));

            if (loan.State != LoanRequestState.Eligible)
                return Result<GetContractFileResultDto>.Failure(new Error(-1, "هنوز مشتری واجد شرایط نشده است."));

            if (cmd.ApprovalCode == 0)
                return Result<GetContractFileResultDto>.Failure(new Error(-1, "کد مصوبه بانک ملت مشخص نیست."));

            var bankRes = await _mediator.Send(cmd, ct);
            var fileName = $"mellat-{bankRes.ContractNumber}.pdf";

            var savedPath = await _contractFileStorage.SaveAsync(loanId, bankRes.ContractFile, fileName, ct);
            var providerType = loan.Provider.ProviderType;

            var policy = _bankPolicyFactory.CreatePolicy<GetContractFileResultDto>(providerType, "ContractFileNoCollateral");

            var decisionResult = policy.Evaluate(bankRes);


            var decision = decisionResult.Value!;

            if (decisionResult.IsSuccess)
            {

                var birthDate = PersianCalendarHelper.ParseShamsiToGregorian(cmd.BirthDate);
                loan.AttachContract(
                    loan.Provider.ApprovalCode,
                    cmd.Address,
                    birthDate,
                    cmd.NationalCode,
                    cmd.InstallmentCount,
                    cmd.LoanAmount,
                    cmd.MobileNumber,
                    cmd.PhoneNumber,
                    cmd.PostalCode,
                    savedPath,
                    bankRes.ContractNumber.ToString(),
                    reasonCode: decision.ReasonCode,
                    uiMessage: decision.UiMessage);

                await _repo.UpdateAsync(loan, ct);

                return Result<GetContractFileResultDto>.Success(new GetContractFileResultDto
                {
                    RequestId = bankRes.RequestId,
                    ContractNumber = bankRes.ContractNumber,
                    ContractPath = savedPath,
                    ContractFile = bankRes.ContractFile,
                    State = loan.State.ToString(),
                    Message = decision.UiMessage,
                    NextActions = GetNextActions(loan)
                });
            }
            else if (decision.Retryable == true)
            {
                return Result<GetContractFileResultDto>.Failure(
                    new Error(decision.ReasonCode, decision.UiMessage ?? "لطفاً بعداً دوباره تلاش کنید."));
            }
            else
            {
                return await FailAndReturn<GetContractFileResultDto>(loan, decisionResult.Error!, ct);
            }
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<GetContractFileResultDto>.Failure(error);

        }
    }


    public async Task<Result<GetCollateralContractFileResultDto>> GetContractFileWithCollateralAsync(Guid loanId, GetCollateralContractFileCommand cmd, CancellationToken ct)
    {
       
            var loan = await RequireAsync(loanId, ct);


            if (loan is null)
                return Result<GetCollateralContractFileResultDto>.Failure(
                    new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));


            if (loan.State != LoanRequestState.Eligible)
                return Result<GetCollateralContractFileResultDto>.Failure(
                    new Error(loan.LastErrorCode, "برای دریافت فایل قرارداد با وثیقه باید مشتری واجد شرایط باشد."));


            if (loan.Provider.ApprovalCode == 0)
                return Result<GetCollateralContractFileResultDto>.Failure(
                    new Error(loan.LastErrorCode, "کد مصوبه بانک ملت مشخص یا معتبر نیست."));

            if (cmd.CollateralType.Equals(2))
                return Result<GetCollateralContractFileResultDto>.Failure(
                    new Error(loan.LastErrorCode, "نوع وثیقه (CHEQUE/PROMISSORY) الزامی است."));


            if (cmd.CollateralNo == 0)
                return Result<GetCollateralContractFileResultDto>.Failure(
                    new Error(loan.LastErrorCode, "شماره و تاریخ وثیقه الزامی است."));
        try
        {

            var bankRes = await _mediator.Send(cmd with { ApprovalCode = Convert.ToDecimal(loan.Provider.ApprovalCode)! }, ct);
            var fileName = $"mellat-{bankRes.ContractNumber}.pdf";

            var savedPath = await _contractFileStorage.SaveAsync(loanId, bankRes.ContractFile, fileName, ct);

            var providerType = loan.Provider.ProviderType;

            var policy = _bankPolicyFactory.CreatePolicy<GetCollateralContractFileResultDto>(providerType, "ContractFileWithCollateral");

            var decision = policy.Evaluate(bankRes);

            if (!decision.IsSuccess)
                return await FailAndReturn<GetCollateralContractFileResultDto>(loan, decision.Error!, ct);

            var d = decision.Value!;

            var birthDate = PersianCalendarHelper.ParseShamsiToGregorian(cmd.BirthDate);

            loan.AttachCollateralContract(
                cmd.CollateralNo,
                cmd.CollateralType,
                cmd.CollateralDate,
                loan.Provider.ApprovalCode,
                cmd.CollateralAmount,
                cmd.Address,
                birthDate,
                cmd.ChequeSerial,
                cmd.CollateralIssuer,
                cmd.GuarantorNC,
                cmd.NationalCode,
                cmd.InstallmentCount,
                cmd.LoanAmount,
                cmd.MobileNumber,
                cmd.PhoneNumber,
                cmd.PostalCode,
                savedPath,
                bankRes.ContractNumber.ToString(),
                reasonCode: d.ReasonCode,
                uiMessage: d.UiMessage);

            await _repo.UpdateAsync(loan, ct);

            return Result<GetCollateralContractFileResultDto>.Success(new GetCollateralContractFileResultDto
            {
                RequestId = bankRes.RequestId,
                ContractNumber = bankRes.ContractNumber,
                ContractPath = savedPath,
                ContractFile = bankRes.ContractFile,
                State = loan.State.ToString(),
                Message = d.UiMessage,
                NextActions = GetNextActions(loan)
            });
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<GetCollateralContractFileResultDto>.Failure(error);

        }
    }


    public async Task<Result<SubmitPayRequestResultDto>> SubmitPayRequestAsync(Guid loanId, SubmitPayRequestCommand cmd, CancellationToken ct)
    {

        var loan = await RequireAsync(loanId, ct);

        if (loan.State != LoanRequestState.ContractsPrepared)
            return Result<SubmitPayRequestResultDto>.Failure(
                new Error(-1, "قرارداد باید در وضعیت آماده (ContractsPrepared) باشد."));


        if (loan.GrantRequest.ContractId < 0)
            return Result<SubmitPayRequestResultDto>.Failure(
                new Error(-1, "شماره قرارداد مشخص نیست."));
        var shouldUpdate = false;
        SubmitPayRequestResultDto res;
        try
        {
            var req = new SubmitPayRequestCommand
            {
                ContractPath = cmd.ContractPath,
                RequestAmount = cmd.RequestAmount,
                ProviderType = cmd.ProviderType,
                ContractNumber = cmd.ContractNumber
            };

            res = await _mediator.Send(req, ct);
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<SubmitPayRequestResultDto>.Failure(error);

        }
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<SubmitPayRequestResultDto>(providerType, "PayRequest");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
            return await FailAndReturn<SubmitPayRequestResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;
        loan.MarkFacilitySubmitted(d.ReasonCode, d.UiMessage);

        if (res.PayRequestId != null)
        {
            loan.SetPayRequestId(res.PayRequestId);
            shouldUpdate = true;
        }

        if (shouldUpdate)
            await _repo.UpdateAsync(loan, ct);

        if (decision.Value.Retryable == true)
        {
            await _jobs.EnqueuePayResponseInquiryAsync(loan.Id, res.PayRequestId, TimeSpan.FromSeconds(30), ct);
        }


        return Result<SubmitPayRequestResultDto>.Success(new SubmitPayRequestResultDto
        {
            RequestId = res.RequestId,
            PayRequestId = res.PayRequestId,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            MessageCode = res.MessageCode,
            NextActions = GetNextActions(loan)
        });

    }

    public async Task<Result<GetPayResponseResultDto>> GetPayResponseAsync(Guid loanId, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
        if (loan is null)
            return Result<GetPayResponseResultDto>.Failure(new Error(-1, "درخواست وام یافت نشد."));

        if (string.IsNullOrWhiteSpace(loan.PayRequest?.PayRequestId))
            return Result<GetPayResponseResultDto>.Failure(new Error(-1, "برای این وام هنوز PayRequestId ثبت نشده است."));

        var providerType = loan.Provider.ProviderType;

        GetPayResponseResultDto res;
        res = await _mediator.Send(new GetPayResponseQuery
        {
            PayRequestId = loan.PayRequest.PayRequestId,
            ProviderType = providerType
        }, ct);


        if (res is null)
            return Result<GetPayResponseResultDto>.Failure(new Error(-1, "پاسخی دریافت نشد."));

        var policy = _bankPolicyFactory.CreatePolicy<GetPayResponseResultDto>(providerType, "PayResponse");
        var decision = policy.Evaluate(res);
        if (!decision.IsSuccess)
            return await FailAndReturn<GetPayResponseResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;


        var code = res.MessageCode ?? 0;

        var shouldUpdate = false;

        switch (code)
        {
            case 2: // success
                var contractNo = res.PayContractInfo?.ContractNo;
                loan.MarkApproved(d.ReasonCode, d.UiMessage, contractNo);
                loan.SetPayResponseApprovedInfo(
                    contractNo: contractNo,
                    loanAmount: res.PayContractInfo?.LoanAmount,
                    contractDate: res.PayContractInfo?.ContractDate,
                    traceCode: res.PayContractInfo?.TraceCode,
                    bankSignedContractBase64: res.PayContractInfo?.ContractFile
                );
                shouldUpdate = true;
                break;

            case 1: // pending
                loan.MarkUnderReview(d.ReasonCode, d.UiMessage);
                shouldUpdate = true;
                break;

            case 3: // failed but retryable
                if (loan.TryIncreasePayResponseRetry(5))
                {
                    loan.MarkTemporaryFailure(d.ReasonCode, d.UiMessage ?? "بانک  پاسخ ناموفق داد، در حال تلاش مجدد.");
                }
                else
                {
                    loan.MarkRejected(d.ReasonCode, d.UiMessage);
                }
                shouldUpdate = true;
                break;

            case 4:
                loan.MarkIneligible(d.ReasonCode, d.UiMessage);
                shouldUpdate = true;
                break;

            default:
                loan.MarkTemporaryFailure(d.ReasonCode, d.UiMessage ?? "پاسخ نامعتبر از بانک.");
                shouldUpdate = true;
                break;
        }

        if (shouldUpdate)
            await _repo.UpdateAsync(loan, ct);

        return Result<GetPayResponseResultDto>.Success(new GetPayResponseResultDto
        {
            RequestId = res.RequestId,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            MessageCode = res.MessageCode,
            NextActions = GetNextActions(loan)
        });
    }


    public async Task<Result<OtpRequestResultDto>> SendOtpAsync(Guid loanId, OtpRequestCommand cmd, CancellationToken ct)
    {

        var loan = await RequireAsync(loanId, ct);

        if (loan is null)
            return Result<OtpRequestResultDto>.Failure(new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));

        if (string.IsNullOrEmpty(loan.Contract.ContractNumber))
            return Result<OtpRequestResultDto>.Failure(new Error(loan.LastErrorCode, "شماره قرارداد معتبر نیست."));


        if (cmd.ServiceType != serviceType.deposit && cmd.ServiceType != serviceType.repayment)
            return Result<OtpRequestResultDto>.Failure(new Error(loan.LastErrorCode, "serviceType باید 1 (واریز) یا 2 (بازپرداخت) باشد."));

        OtpRequestResultDto res;
        try
        {
            res = await _mediator.Send(cmd with
            {
                ContractNumber = cmd.ContractNumber,
                NationalCode = cmd.NationalCode,
                AccountNumber = cmd.AccountNumber,
                PayAmount = cmd.PayAmount,
                ProviderType = cmd.ProviderType,
                ServiceType = cmd.ServiceType
            }, ct);

        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<OtpRequestResultDto>.Failure(error);

        }
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<OtpRequestResultDto>(providerType, "OtpRequest");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
            return await FailAndReturn<OtpRequestResultDto>(loan, decision.Error!, ct);


        if (decision.Value.Retryable == true)
        {

            const int maxRetry = 5;

            if (loan.RetryCount < maxRetry)
            {
                loan.RetryCount++;
                loan.TransitionTo(decision.Value.NextState ?? LoanRequestState.OtpSent, decision.Value.ReasonCode, decision.Value.UiMessage);
                await _repo.UpdateAsync(loan, ct);

                await _jobs.EnqueueOtpRequestRetryAsync(loanId, cmd, TimeSpan.FromSeconds(30), ct);
            }
            else
            {

                loan.State = LoanRequestState.Failed;
                await _repo.UpdateAsync(loan, ct);
            }
        }
        if (loan.State != LoanRequestState.Failed)
        {
            loan.MarkOtpSent(decision.Value.ReasonCode, decision.Value.UiMessage);
            await _repo.UpdateAsync(loan, ct);
        }

        return Result<OtpRequestResultDto>.Success(new OtpRequestResultDto
        {
            LoanRequestId = loan.Id,
            State = loan.State.ToString(),
            MessageCode = res.MessageCode,
            Message = decision.Value.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }


    public async Task<Result<DepositRequestResultDto>> DepositRequestAsync(Guid loanId, DepositRequestCommand cmd, CancellationToken ct)
    {

        var loan = await RequireAsync(loanId, ct);

        if (loan is null)
            return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));

        if (loan.Contract.ApprovalCode <= 0)
            return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, "شماره قرارداد معتبر نیست."));


        if (loan.IsPurchaseCredit)
            return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, "درخواست واریز وجه صرفاً برای اعتبار در خرید مجاز است."));


        if (loan.RequiresOtp && loan.State is not LoanRequestState.OtpVerified and not LoanRequestState.RemittanceRegistering)
            return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, "برای ثبت واریز، تایید رمز یکبار مصرف الزامی است."));



        if (!Enum.IsDefined(typeof(depositType), cmd.DepositType))
            return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, $"نوع حساب باید یکی از مقادیر ({depositType.AnyAccount} ,{depositType.FixedAccount},{depositType.CustomerAccount}) باشد."));

        DepositRequestResultDto res;
        try
        {


            res = await _mediator.Send(cmd, ct);
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<DepositRequestResultDto>.Failure(error);

        }
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<DepositRequestResultDto>(providerType, "DepositRequest");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
        {
            if (decision.Value?.Retryable == true)
            {

                const int maxRetry = 5;

                if (loan.RetryCount < maxRetry)
                {
                    loan.RetryCount++;
                    loan.TransitionTo(LoanRequestState.Failed, decision.Value.ReasonCode, decision.Value.UiMessage);
                    await _repo.UpdateAsync(loan, ct);

                    await _jobs.EnqueueDepositRetryAsync(loanId, TimeSpan.FromSeconds(30), ct, cmd);
                    return Result<DepositRequestResultDto>.Failure(new Error(decision.Value.ReasonCode, "در حال تلاش مجدد برای واریز وجه..."));
                }
                else
                {

                    loan.State = LoanRequestState.Failed;
                    await _repo.UpdateAsync(loan, ct);
                    return Result<DepositRequestResultDto>.Failure(new Error(decision.Value.ReasonCode, decision.Value.UiMessage));
                }

            }

            return await FailAndReturn<DepositRequestResultDto>(loan, decision.Error!, ct);
        }

        var d = decision.Value!;
        loan.MarkRemittanceRegistering(d.ReasonCode, d.UiMessage);
        loan.SetTransactionNumber(res.TransactionNumber);
        await _repo.UpdateAsync(loan, ct);

        return Result<DepositRequestResultDto>.Success(new DepositRequestResultDto
        {
            RequestId = res.RequestId,
            TransactionNumber = res.TransactionNumber,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            NextActions = GetNextActions(loan)
        });


    }

    public async Task<Result<RepaymentRequestResultDto>> RepaymentRequestAsync(Guid loanId, RepaymentRequestCommand cmd, CancellationToken ct)
    {

        var loan = await RequireAsync(loanId, ct);
        if (loan is null)
            return Result<RepaymentRequestResultDto>.Failure(new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));

        if (loan.Contract.ApprovalCode <= 0)
            return Result<RepaymentRequestResultDto>.Failure(new Error(loan.LastErrorCode, "شماره قرارداد معتبر نیست."));


        if (loan.State is not LoanRequestState.Approved and not LoanRequestState.Disbursed)
            return Result<RepaymentRequestResultDto>.Failure(new Error(loan.LastErrorCode, "در این وضعیت ثبت بازپرداخت مجاز نیست."));


        if (!loan.CanPerformRepayment(cmd.OtpCode))
            return Result<RepaymentRequestResultDto>.Failure(
                new Error(loan.LastErrorCode, "براساس تنظیمات مصوبه، رمز دوم الزامی است."));
        RepaymentRequestResultDto res;
        try
        {

            res = await _mediator.Send(cmd, ct);
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<RepaymentRequestResultDto>.Failure(error);

        }
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<RepaymentRequestResultDto>(providerType, "RepaymentRequest");

        var decision = policy.Evaluate(res);
        if (!decision.IsSuccess)
            return await FailAndReturn<RepaymentRequestResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;
        loan.MarkRepaymentRegistered(d.ReasonCode, d.UiMessage, res.TrackNumber.ToString(), res.AccountNumber.ToString(), res.RepaymentAmount, res.RepaymentDate);

        //loan.SetLastRepaymentInfo(
        //     trackNumber: res.TrackNumber?.ToString(),
        //     accountNo: res.AccountNumber?.ToString(),
        //     amount: res.RepaymentAmount,
        //     whenUtc: res.RepaymentDate?.ToUniversalTime());
        await _repo.UpdateAsync(loan, ct);

        return Result<RepaymentRequestResultDto>.Success(new RepaymentRequestResultDto
        {
            RequestId = res.RequestId,
            TrackNumber = res.TrackNumber,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            NextActions = GetNextActions(loan)
        });

    }


    public async Task<Result<GetInstallmentsResultDto>> GetInstallmentsAsync(Guid loanId, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);


        if (loan.Contract is null || string.IsNullOrEmpty(loan.Contract.ContractNumber))
            return Result<GetInstallmentsResultDto>.Failure(new Error(-2, "قرارداد برای این درخواست ثبت نشده است."));


        var providerType = loan.Provider.ProviderType;

        loan.TransitionTo(LoanRequestState.InstallmentsFetching, null, "دریافت اقساط از بانک.");
        await _repo.UpdateAsync(loan, ct);


        var res = await _mediator.Send(new GetInstallmentsQuery
        {
            NationalCode = loan.Customer.NationalCode,
            ContractNumber = loan.Contract.ContractNumber,
            ProviderType = providerType
        }, ct);


        var policy = _bankPolicyFactory.CreatePolicy<GetInstallmentsResultDto>(providerType, "Installments");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
        {

            if (decision.Value?.Retryable == true)
            {
                loan.TransitionTo(LoanRequestState.InstallmentsRetryPending, decision.Value.ReasonCode, decision.Value.UiMessage);
                await _repo.UpdateAsync(loan, ct);

                await _jobs.EnqueueRetryGetInstallment(loanId, TimeSpan.FromMinutes(15), ct);
                return Result<GetInstallmentsResultDto>.Failure(new Error(decision.Value!.ReasonCode, decision.Value!.UiMessage ?? "اقساط هنوز اماده نیست در حال تلاش مجدد "));
            }
            var err = decision.Error
              ?? new Error(decision.Value?.ReasonCode ?? -1, decision.Value?.UiMessage ?? "خطای نامشخص در دریافت اقساط.");

            return await FailAndReturn<GetInstallmentsResultDto>(loan, err, ct);
        }

        if (res.Installments is not null && res.Installments.Any())
        {
            var installments = _mapper.Map<List<InstallmentStatus>>(res.Installments);

            var contractNo = res.ContractNumber != 0m ? res.ContractNumber : decimal.Parse(loan.Contract.ContractNumber);

            foreach (var item in installments)
                item.ContractNumber = contractNo;

            await _repo.InsertInstallmentAsync(installments, loanId, ct);
        }

        loan.TransitionTo(LoanRequestState.InstallmentsFetched, decision.Value?.ReasonCode, decision.Value?.UiMessage ?? "اقساط با موفقیت دریافت شد.");
        await _repo.UpdateAsync(loan, ct);

        return Result<GetInstallmentsResultDto>.Success(new GetInstallmentsResultDto
        {
            RequestId = res.RequestId,
            State = loan.State.ToString(),
            Installments = res.Installments,
            Message = decision.Value!.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }


    public async Task<Result<GetCustomerCreditBalanceResultDto>> GetCreditBalanceAsync(Guid loanId, CancellationToken ct)
    {

        var loan = await RequireAsync(loanId, ct);

        if (loan is null)
            return Result<GetCustomerCreditBalanceResultDto>.Failure(new Error(-1, "درخواست وام یافت نشد."));

        if (loan.Contract is null || string.IsNullOrEmpty(loan.Contract.ContractNumber))
            return Result<GetCustomerCreditBalanceResultDto>.Failure(new Error(-2, "برای این درخواست، قرارداد نهایی نشده است."));

        var providerType = loan.Provider.ProviderType;
        GetCustomerCreditBalanceResultDto res;
        try
        {
            res = await _mediator.Send(new GetCustomerCreditBalanceCommand
            {
                NationalCode = loan.Customer.NationalCode,
                ContractNumber = loan.Contract.ContractNumber,
                ProviderType = providerType
            }, ct);
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<GetCustomerCreditBalanceResultDto>.Failure(error);

        }
        var policy = _bankPolicyFactory.CreatePolicy<GetCustomerCreditBalanceResultDto>(providerType, "CommonRules");


        var decision = policy.Evaluate(res);
        if (!decision.IsSuccess)
        {

            if (decision.Value is not null && decision.Value.Retryable == true)
            {
                loan.TransitionTo(LoanRequestState.InProgress, decision.Value.ReasonCode, decision.Value.UiMessage);

                await _repo.UpdateAsync(loan, ct);

                await _jobs.EnqueueRetryCreditBalance(loanId, TimeSpan.FromSeconds(45), ct);

                return Result<GetCustomerCreditBalanceResultDto>.Failure(
                                   new Error(decision.Value.ReasonCode, decision.Value.UiMessage));
            }
            if (decision.Value.NextState is not null)
            {
                loan.TransitionTo(decision.Value.NextState.Value, decision.Value.ReasonCode, decision.Value.UiMessage);
            }

            await _repo.UpdateAsync(loan, ct);

            return await FailAndReturn<GetCustomerCreditBalanceResultDto>(loan, new Error(decision.Value.ReasonCode, decision.Value.UiMessage), ct);

        }

        loan.TransitionTo(LoanRequestState.CreditChecked, decision.Value.ReasonCode, decision.Value.UiMessage ?? "مانده حساب با موفقیت دریافت شد.");
        await _repo.UpdateAsync(loan, ct);

        return Result<GetCustomerCreditBalanceResultDto>.Success(new GetCustomerCreditBalanceResultDto
        {
            NationalCode = res.NationalCode,
            contractCreditList = res.contractCreditList,
            State = loan.State.ToString(),
            Message = decision.Value!.UiMessage,
            MessageCode = res.MessageCode,
            NextActions = GetNextActions(loan)
        });

    }

    public async Task<Result<GetCustomerBillingResultDto>> GetBillingAsync(Guid loanId, GetCustomerBillingCommand cmd, CancellationToken ct)
    {

        var loan = await RequireAsync(loanId, ct);
        if (loan is null)
            return Result<GetCustomerBillingResultDto>.Failure(
                new Error(-1, "درخواست وام یافت نشد."));

        GetCustomerBillingResultDto res;
        try
        {
            res = await _mediator.Send(cmd with { ContractNumber = loan.Contract.ContractNumber, NationalCode = loan.Customer.NationalCode! }, ct);
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<GetCustomerBillingResultDto>.Failure(error);

        }
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<GetCustomerBillingResultDto>(providerType, "CustomerBilling");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
            return await FailAndReturn<GetCustomerBillingResultDto>(loan, decision.Error!, ct);

        return Result<GetCustomerBillingResultDto>.Success(new GetCustomerBillingResultDto
        {
            RequestId = res.RequestId,
            Billings = res.Billings,
            State = loan.State.ToString(),
            Message = decision.Value!.UiMessage,
            NextActions = GetNextActions(loan)
        });

    }


    public async Task<Result<GetCustomerPurchaseDetailsResultDto>> GetPurchasesAsync(Guid loanId, GetCustomerPurchaseDetailsCommand cmd, CancellationToken ct)
    {

        var loan = await RequireAsync(loanId, ct);
        if (loan is null)
            return Result<GetCustomerPurchaseDetailsResultDto>.Failure(
                new Error(-1, "درخواست وام یافت نشد."));

        if (loan.Provider is null)
            return Result<GetCustomerPurchaseDetailsResultDto>.Failure(
                new Error(-1, "برای این درخواست وام، بانک ارائه‌دهنده ثبت نشده است."));


        if (string.IsNullOrEmpty(loan.Contract.ContractNumber))
            return Result<GetCustomerPurchaseDetailsResultDto>.Failure(
                new Error(-1, "برای این وام قرارداد معتبری ثبت نشده است."));


        if (string.IsNullOrWhiteSpace(loan.Customer?.NationalCode))
            return Result<GetCustomerPurchaseDetailsResultDto>.Failure(
                new Error(-1, "کد ملی مشتری برای این وام موجود نیست."));

        var providerType = loan.Provider.ProviderType;
        GetCustomerPurchaseDetailsResultDto res;
        try
        {
            res = await _mediator.Send(cmd with
            {
                ContractNumber = loan.Contract.ContractNumber,
                NationalCode = loan.Customer.NationalCode!,
                ProviderType = providerType
            }, ct);
        }
        catch (ValidationException ex)
        {

            var details = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(e => e.ErrorMessage)
                                                .Distinct()
                                                .ToArray()
                                          );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);

            return Result<GetCustomerPurchaseDetailsResultDto>.Failure(error);

        }
        if (res is null)
            return Result<GetCustomerPurchaseDetailsResultDto>.Failure(
                new Error(-1, "پاسخی از سرویس دریافت نشد."));

        var policy = _bankPolicyFactory.CreatePolicy<GetCustomerPurchaseDetailsResultDto>(providerType, "PurchaseDetails");
        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
            return await FailAndReturn<GetCustomerPurchaseDetailsResultDto>(loan, decision.Error!, ct);

        return Result<GetCustomerPurchaseDetailsResultDto>.Success(new GetCustomerPurchaseDetailsResultDto
        {
            RequestId = res.RequestId,
            ContractDetails = res.ContractDetails,
            State = loan.State.ToString(),
            Message = decision.Value!.UiMessage,
            NextActions = GetNextActions(loan)
        });

    }


    public async Task<Result<TransferRegisterResultDto>> TransferRegisterAsync(Guid loanId, TransferRegisterCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
        if (loan is null)
            return Result<TransferRegisterResultDto>.Failure(
                new Error(-1, "درخواست وام یافت نشد."));

        if (loan.Provider is null)
            return Result<TransferRegisterResultDto>.Failure(
                new Error(-1, "بانک ارائه‌دهنده برای این درخواست وام ثبت نشده است."));

        var providerType = loan.Provider.ProviderType;

        TransferRegisterResultDto res;
        try
        {
            res = await _mediator.Send(cmd, ct);
        }
        catch (ValidationException ex)
        {
            var details = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage)
                          .Distinct()
                          .ToArray()
                );

            var error = new Error(400, "ورودی‌ها نامعتبر هستند.", details);
            return Result<TransferRegisterResultDto>.Failure(error);
        }
        if (res is null)
            return Result<TransferRegisterResultDto>.Failure(
                new Error(-1, "پاسخی دریافت نشد."));

        var policy = _bankPolicyFactory.CreatePolicy<TransferRegisterResultDto>(providerType, "TransferRegister");
        var decision = policy.Evaluate(res);
        if (!decision.IsSuccess)
            return await FailAndReturn<TransferRegisterResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;


        loan.MarkRemittanceRegistering(d.ReasonCode, d.UiMessage);
        loan.SetRegisterCode(res.RegisterCode);
        await _repo.UpdateAsync(loan, ct);


         await _jobs.EnqueueTransferInquiryAsync(loan.Id, TimeSpan.FromMinutes(15), ct);

        return Result<TransferRegisterResultDto>.Success(new TransferRegisterResultDto
        {
            RequestId = res.RequestId,
            RegisterCode = res.RegisterCode,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }

    public async Task<Result<TransferInquiryResultDto>> TransferInquiryAsync(Guid loanId, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
        if (loan is null)
            return Result<TransferInquiryResultDto>.Failure(
                new Error(-1, "درخواست وام یافت نشد."));

        if (loan.Transfer is null || string.IsNullOrWhiteSpace(loan.Transfer.RegisterCode))
           return Result<TransferInquiryResultDto>.Failure(
                new Error(-1, "شماره پیگیری یافت نشد."));

        var res = await _mediator.Send(new TransferInquiryQuery { RegisterCode = loan.Transfer.RegisterCode! }, ct);

        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<TransferInquiryResultDto>(providerType, "TransferInquiry");

        var decision = policy.Evaluate(res);
        if (!decision.IsSuccess)
            return await FailAndReturn<TransferInquiryResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;

        var detail = res.InquiryDetails?.FirstOrDefault();

        if (detail is null)
        {
            loan.MarkRemittancePending(d.ReasonCode, d.UiMessage ?? "بانک بدون جزئیات حواله پاسخ داد.");
        }
        else
        {
            switch (detail.transferStatus)
            {
                case TransferInquiryResultDto.TransferStatus.Registered:
                    loan.MarkDisbursed(d.ReasonCode, d.UiMessage);
                    break;

                case TransferInquiryResultDto.TransferStatus.Returned:
                case TransferInquiryResultDto.TransferStatus.CanceledAndReturned:
                    loan.MarkRemittanceReturned(d.ReasonCode, d.UiMessage);
                    break;

                case TransferInquiryResultDto.TransferStatus.Unsendable:
                    loan.MarkRemittanceFailed(d.ReasonCode, d.UiMessage);
                    break;

                default:
                    loan.MarkRemittancePending(d.ReasonCode, d.UiMessage);
                    break;
            }
        }

        await _repo.UpdateAsync(loan, ct);

        return Result<TransferInquiryResultDto>.Success(new TransferInquiryResultDto
        {
            RequestId = res.RequestId,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }




    // --------- Helpers ----------
    private async Task<LoanRequest> RequireAsync(Guid id, CancellationToken ct)
        => await _repo.GetByIdAsync(id, ct) ?? throw new InvalidOperationException("درخواست وام یافت نشد.");

    private async Task<Result<T>> FailAndReturn<T>(LoanRequest e, Error error, CancellationToken ct)
    {
        e.MarkTemporaryFailure(error.Code, error.Message);
        await _repo.UpdateAsync(e, ct);
        return Result<T>.Failure(error);
    }

    private static string[] GetNextActions(LoanRequest e) => NextActionsBuilder.From(e);
}

public static class NextActionsBuilder
{
    public static string[] From(LoanRequest e)
    {
        var id = e.Id;
        return e.State switch
        {
            LoanRequestState.KYCChecked => new[] { $"GET /api/v1/loan-requests/{id}/inquiry/result" },
            LoanRequestState.Eligible => new[] { $"POST /api/v1/loan-requests/{id}/contracts/file" },
            LoanRequestState.ContractsPrepared => new[] { $"POST /api/v1/loan-requests/{id}/pay-request" },
            LoanRequestState.FacilitySubmitted => new[] { $"GET /api/v1/loan-requests/{id}/pay-response" },
            LoanRequestState.Approved => new[] { $"POST /api/v1/loan-requests/{id}/transfer/register" },
            LoanRequestState.RemittancePending => new[] { $"GET /api/v1/loan-requests/{id}/transfer/inquiry" },
            LoanRequestState.Disbursed => new[] { $"GET /api/v1/loan-requests/{id}/installments" },
            _ => new[] { $"GET /api/v1/loan-requests/{id}" }
        };
    }
}








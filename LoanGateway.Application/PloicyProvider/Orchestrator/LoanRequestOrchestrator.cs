using Bank.Mellat.Provider.Dtos;
using Common;
using Hangfire;
using LoanService.Application.Contracts;
using LoanService.Application.Exception;
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
using LoanService.Domain.Entities;
using LoanService.Domain.Enum;
using LoanService.Domain.IRepository;
using Mapster;
using MapsterMapper;
using MediatR;
using static LoanService.Application.UseCase.Command.OtpRequest.OtpRequestCommand;
using PayResponseCode = LoanService.Domain.Entities.PayResponseCode;



namespace LoanService.Application.UseCase.Command.StartLoanRequestDto;

public sealed class LoanRequestOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILoanRequestRepository _repo;
    private readonly ILoanOrchestratorJobRunner _jobs;
    private readonly IBankPolicyFactory _bankPolicyFactory;
    private readonly IMapper _mapper;
    private readonly IContractFileStorage _contractFileStorage;
    public LoanRequestOrchestrator(
        IMediator mediator,
        ILoanRequestRepository repo,
        ILoanOrchestratorJobRunner jobs,
        IMapper mapper,
        IBankPolicyFactory bankPolicyFactory,
        IContractFileStorage contractFileStorage)

    {
        _mapper = mapper;
        _mediator = mediator;
        _repo = repo;
        _jobs = jobs;
        _bankPolicyFactory = bankPolicyFactory;
        _contractFileStorage = contractFileStorage;

    }

    // ---------------- 1) ثبت درخواست استعلام مشتری ----------------
    public async Task<Result<CustomerInquiryResultDto>> StartInquiryAsync(CustomerInquiryCommand cmd, LoanRequest loan, CancellationToken ct)
    {
       try
        {
            var bankRes = await _mediator.Send(cmd, ct);

            var providerType = cmd.ProviderType;

            var policy = _bankPolicyFactory.CreatePolicy<CustomerInquiryResultDto>(providerType, "CustomerInquiry");

            var decisionResult = policy.Evaluate(bankRes);

            if (!decisionResult.IsSuccess)
                return await FailAndReturn<CustomerInquiryResultDto>(
                    loan,
                    decisionResult.Error!,
                    ct);

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
                new CustomerInquiryResultDto {
                    RequestId = bankRes.RequestId,
                    Message=bankRes.Message,
                    MessageCode=bankRes.MessageCode,
                    State=bankRes.State
                });
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

    // ---------------- 2) پاسخ استعلام مشتری (allowed/maxApprovedAmount/...) --------------
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
            ProviderType= ProviderType
        }, ct);

        var providerType = ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<CustomerInquiryStatusResultDto>( providerType,  "CustomerInquiryResult");

        var decisionResult = policy.Evaluate(bankRes);
        if (!decisionResult.IsSuccess)
        {
            
            loan.TransitionTo(LoanRequestState.Failed, decisionResult.Error!.Code, decisionResult.Error.Message);

            await _repo.UpdateAsync(loan, ct);

            return Result<CustomerInquiryStatusResultDto>.Failure(decisionResult.Error);
        }
        var decision = decisionResult.Value!;

        if (decision.Retryable==true)
        {
         
            loan.TransitionTo(
                decision.NextState ?? LoanRequestState.KYCChecked,
                decision.ReasonCode,
                decision.UiMessage);

            await _repo.UpdateAsync(loan, ct);

           
             await _jobs.EnqueueInquiryResultRetryAsync(loan.Id, providerType,TimeSpan.FromSeconds(30), ct);

            return Result<CustomerInquiryStatusResultDto>.Failure(
                new Error(decision.ReasonCode, decision.UiMessage ?? "نتیجه استعلام هنوز آماده نیست."));
        }

        loan.SetInquiryDecision(bankRes.Allowed, bankRes.MaxApprovedAmount, bankRes.StatusList.Select(x=>new InquiryInfo.StatusItem
        {
            ResponseCode=x.ResponseCode,
            ResponseStatus=x.ResponseStatus
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
            StatusList =bankRes.StatusList.Select(x=>new CustomerInquiryStatusResultDto.StatusItemDto(x.ResponseStatus,x.ResponseCode)).ToList(),
            NextActions = GetNextActions(loan)
        });
    }

    // ---------------- 3) دریافت فایل قرارداد بدون وثیقه ----------------
    public async Task<Result<GetContractFileResultDto>> GetContractFileNoCollateralAsync(Guid loanId, GetContractFileCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);

        if (loan is null)
            return Result<GetContractFileResultDto>.Failure(new Error(-1, "درخواست وام یافت نشد."));

        if (loan.State != LoanRequestState.Eligible)
            return Result<GetContractFileResultDto>.Failure(new Error(-1, "هنوز مشتری واجد شرایط نشده است."));

        if (cmd.ApprovalCode==0)
            return Result<GetContractFileResultDto>.Failure(new Error(-1, "کد مصوبه بانک ملت مشخص نیست."));

        var bankRes = await _mediator.Send(cmd, ct);
        var fileName = $"mellat-{bankRes.ContractNumber}.pdf";

        var savedPath = await _contractFileStorage.SaveAsync(loanId, bankRes.ContractFile, fileName, ct);
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<GetContractFileResultDto>( providerType,"ContractFileNoCollateral");

        var decisionResult = policy.Evaluate(bankRes);

  
        var decision = decisionResult.Value!;

        if (decisionResult.IsSuccess)
        {
          
            var birthDate = DateOnly.ParseExact(cmd.BirthDate, "yyyy/MM/dd");
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
                reasonCode: decision.ReasonCode,
                uiMessage: decision.UiMessage);

            await _repo.UpdateAsync(loan, ct);

            return Result<GetContractFileResultDto>.Success(new GetContractFileResultDto
            {
                RequestId = bankRes.RequestId,
                ContractNumber = bankRes.ContractNumber,
                ContractPath = savedPath,
                ContractFile=bankRes.ContractFile,
                State = loan.State.ToString(),
                Message = decision.UiMessage,
                NextActions = GetNextActions(loan)
            });
        }
        else if(decision.Retryable==true)
        {
            return Result<GetContractFileResultDto>.Failure(
                new Error(decision.ReasonCode  , decision.UiMessage ?? "لطفاً بعداً دوباره تلاش کنید."));
        }
        else
       {
            return await FailAndReturn<GetContractFileResultDto>(loan, decisionResult.Error!, ct);
        }
    }

    // ---------------- 4) دریافت فایل قرارداد با وثیقه ----------------
    public async Task<Result<GetCollateralContractFileResultDto>> GetContractFileWithCollateralAsync(Guid loanId, GetCollateralContractFileCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
     

        if (loan is null)
            return Result<GetCollateralContractFileResultDto>.Failure(
                new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));


        if (loan.State != LoanRequestState.Eligible)
            return Result<GetCollateralContractFileResultDto>.Failure(
                new Error(loan.LastErrorCode, "برای دریافت فایل قرارداد با وثیقه باید مشتری واجد شرایط باشد."));


        if (loan.Provider.ApprovalCode==0)
            return Result<GetCollateralContractFileResultDto>.Failure(
                new Error(loan.LastErrorCode, "کد مصوبه بانک ملت مشخص یا معتبر نیست."));

        if (cmd.CollateralType.Equals(2))
            return Result<GetCollateralContractFileResultDto>.Failure(
                new Error(loan.LastErrorCode, "نوع وثیقه (CHEQUE/PROMISSORY) الزامی است."));


        if (cmd.CollateralNo==0)
            return Result<GetCollateralContractFileResultDto>.Failure(
                new Error(loan.LastErrorCode, "شماره و تاریخ وثیقه الزامی است."));


        var bankRes = await _mediator.Send(cmd with { ApprovalCode = Convert.ToDecimal(loan.Provider.ApprovalCode)! }, ct);
        var fileName = $"mellat-{bankRes.ContractNumber}.pdf";

        var savedPath = await _contractFileStorage.SaveAsync(loanId, bankRes.ContractFile, fileName, ct);

        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<GetCollateralContractFileResultDto>(providerType, "ContractFileWithCollateral");

        var decision = policy.Evaluate(bankRes);

        if (!decision.IsSuccess)
            return await FailAndReturn<GetCollateralContractFileResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;

        var birthDate = DateOnly.ParseExact(cmd.BirthDate, "yyyy/MM/dd");
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

    // ---------------- 5) ثبت درخواست اعطای تسهیلات (آپلود فایل امضا شده) --------------
    public async Task<Result<SubmitPayRequestResultDto>> SubmitPayRequestAsync(Guid loanId, SubmitPayRequestCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);

        var path = loan.Contract.ContractPath;

        var fileBytes = await _contractFileStorage.ReadAsync(path, ct);

        if (loan.State != LoanRequestState.ContractsPrepared)
            return Result<SubmitPayRequestResultDto>.Failure(
                new Error(loan.LastErrorCode, "قرارداد باید در وضعیت آماده (ContractsPrepared) باشد."));


        if (loan.GrantRequest.ContractId<0)
            return Result<SubmitPayRequestResultDto>.Failure(
                new Error(loan.LastErrorCode, "شماره قرارداد مشخص نیست."));

        var req = new SubmitPayRequestCommand
        {
            ContractFile = fileBytes,
            ContractNumber = cmd.ContractNumber,
            LoanRequestId = loanId,
            RequestAmount = cmd.RequestAmount,
            ProviderType = cmd.ProviderType
        };

        var res = await _mediator.Send(req, ct);

        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<SubmitPayRequestResultDto>(providerType, "PayRequest");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
            return await FailAndReturn<SubmitPayRequestResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;
        loan.MarkFacilitySubmitted(d.ReasonCode, d.UiMessage);

        loan.SetPayRequestId(res.PayRequestId);

        await _repo.UpdateAsync(loan, ct);


        await _jobs.EnqueuePayResponseInquiryAsync(loan.Id, res.PayRequestId, TimeSpan.FromSeconds(30), ct);

        return Result<SubmitPayRequestResultDto>.Success(new SubmitPayRequestResultDto
        {
            RequestId = res.RequestId,
            PayRequestId = res.PayRequestId,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }

    // ---------------- 6) پاسخ اعطا ----------------
    public async Task<Result<GetPayResponseResultDto>> GetPayResponseAsync(Guid loanId, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);

        if (loan is null)
            return Result<GetPayResponseResultDto>.Failure(new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));

        if (string.IsNullOrWhiteSpace(loan.GrantRequest.PayRequestId))
            return Result<GetPayResponseResultDto>.Failure(new Error(loan.LastErrorCode, "برای این وام هنوز PayRequestId ثبت نشده است."));

        var res = await _mediator.Send(new GetPayResponseQuery { PayRequestId = loan.GrantRequest.PayRequestId }, ct);



        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<GetPayResponseResultDto>(providerType, "PayResponse");

        var decision = policy.Evaluate(res);
        if (!decision.IsSuccess)
            return await FailAndReturn<GetPayResponseResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;

        var code = res.PayRequestStatus?.ResponseCode ?? PayResponseCode.Unknown;

        switch (code)
        {
            case PayResponseCode.Success:
                var contractNo = res.PayContractInfo.ContractNo;
                loan.MarkApproved(d.ReasonCode, d.UiMessage, contractNo);


                loan.SetPayResponseApprovedInfo(
                    contractNo: contractNo,
                    loanAmount: res.PayContractInfo?.LoanAmount,
                    contractDate: res.PayContractInfo?.ContractDate,
                    traceCode: res.PayContractInfo?.TraceCode,
                    bankSignedContractBase64: res.PayContractInfo?.ContractFile
                );
                //loan.SetOtpRequirement(res.PayContractInfo?.ot == true);
                break;

            case PayResponseCode.Pending:
                loan.MarkUnderReview(d.ReasonCode, d.UiMessage);
                break;

            case PayResponseCode.Failed:
                loan.MarkRejected(d.ReasonCode, d.UiMessage);
                break;

            case PayResponseCode.NotAllowed:
                loan.MarkIneligible(d.ReasonCode, d.UiMessage);
                break;

            default:

                loan.MarkTemporaryFailure(d.ReasonCode, d.UiMessage ?? "پاسخ نامعتبر از بانک.");
                break;
        }

        await _repo.UpdateAsync(loan, ct);

        return Result<GetPayResponseResultDto>.Success(new GetPayResponseResultDto
        {
            RequestId = res.RequestId,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }

    // ---------------- 7) ارسال OTP (برای Deposit/Repayment) --------------
    public async Task<Result<OtpRequestResultDto>> SendOtpAsync(Guid loanId, OtpRequestCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);

        if (loan is null)
            return Result<OtpRequestResultDto>.Failure(new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));

        //if (loan.Contract.ContractNumber<=0 )
        //    return Result<OtpRequestResultDto>.Failure(new Error(loan.LastErrorCode, "شماره قرارداد معتبر نیست."));


        if (loan.State is not LoanRequestState.ContractsPrepared and not LoanRequestState.OtpSent)
            return Result<OtpRequestResultDto>.Failure(new Error(loan.LastErrorCode, "در این وضعیت ارسال OTP مجاز نیست."));

        if (cmd.ServiceType != serviceType.deposit && cmd.ServiceType != serviceType.repayment)
            return Result<OtpRequestResultDto>.Failure(new Error(loan.LastErrorCode, "serviceType باید 1 (واریز) یا 2 (بازپرداخت) باشد."));

        if (cmd.ServiceType == serviceType.repayment && string.IsNullOrWhiteSpace(cmd.AccountNumber))
            return Result<OtpRequestResultDto>.Failure(new Error(loan.LastErrorCode, "برای serviceType=2، accountNumber الزامی است."));


        var res = await _mediator.Send(cmd with
        {
            //ContractNumber = loan.GrantRequest.ContractId
        }, ct);
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<OtpRequestResultDto>(providerType, "OtpRequest");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
            return await FailAndReturn<OtpRequestResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;
        loan.MarkOtpSent(d.ReasonCode, d.UiMessage);
        await _repo.UpdateAsync(loan, ct);

        return Result<OtpRequestResultDto>.Success(new OtpRequestResultDto
        {
            LoanRequestId = loan.Id,
            State = loan.State.ToString(),
            Message = d.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }

    /// <summary>
    /// ) درخواست واریز وجه (اعتبار در خرید
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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
            return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, "برای ثبت واریز، تایید OTP الزامی است."));

        if (cmd.PayAmount <= 0)
            return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, "payAmount باید بزرگ‌تر از صفر باشد."));

        if (!Enum.IsDefined(typeof(depositType), cmd.DepositType))
            return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, $"depositType باید یکی از مقادیر ({depositType.AnyAccount} ,{depositType.FixedAccount},{depositType.CustomerAccount}) باشد."));


        if (cmd.DepositType == depositType.AnyAccount)
        {
            if (cmd.SellerAccountNo < 0)
                return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, "برای depositType=1، شماره حساب فروشنده الزامی است."));

            if (string.IsNullOrWhiteSpace(cmd.SellerNationalCode))
                return Result<DepositRequestResultDto>.Failure(new Error(loan.LastErrorCode, "برای depositType=1، کد ملی فروشنده الزامی است."));
        }

        var res = await _mediator.Send(cmd, ct);
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<DepositRequestResultDto>(providerType, "DepositRequest");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
        {
            if (decision.Value?.Retryable == true)
            {
                loan.TransitionTo(LoanRequestState.Failed, decision.Value.ReasonCode, decision.Value.UiMessage);
        
                _jobs.EnqueueDepositRetryAsync(loan.Id, TimeSpan.FromMinutes(5), ct,cmd);

                return Result<DepositRequestResultDto>.Failure(
                    new Error(decision.Value.ReasonCode, "در حال تلاش مجدد برای واریز وجه...")
                );
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

    // ---------------- 9) بازپرداخت از حساب ملت --------------
    public async Task<Result<RepaymentRequestResultDto>> RepaymentRequestAsync(Guid loanId, RepaymentRequestCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
        if (loan is null)
            return Result<RepaymentRequestResultDto>.Failure(new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));

        if (loan.Contract.ApprovalCode<=0 )
            return Result<RepaymentRequestResultDto>.Failure(new Error(loan.LastErrorCode, "شماره قرارداد معتبر نیست."));


        if (loan.State is not LoanRequestState.Approved and not LoanRequestState.Disbursed)
            return Result<RepaymentRequestResultDto>.Failure(new Error(loan.LastErrorCode, "در این وضعیت ثبت بازپرداخت مجاز نیست."));

        if (string.IsNullOrWhiteSpace(cmd.AccountNo))
            return Result<RepaymentRequestResultDto>.Failure(new Error(loan.LastErrorCode, "accountNo الزامی است."));

        if (cmd.RepaymentAmount <= 0)
            return Result<RepaymentRequestResultDto>.Failure(new Error(loan.LastErrorCode, "repaymentAmount باید بزرگ‌تر از صفر باشد."));

        if (!loan.CanPerformRepayment(cmd.OtpCode))
            return Result<RepaymentRequestResultDto>.Failure(
                new Error(loan.LastErrorCode, "براساس تنظیمات مصوبه، OTP الزامی است."));

        var res = await _mediator.Send(cmd with
        {
            ContractNo = loan.Contract.ApprovalCode,
            NationalCode = loan.Customer.NationalCode!
        }, ct);

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

    /// <summary>
    /// مشاهده وضعیت و اقساط قرارداد 
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<GetInstallmentsResultDto>> GetInstallmentsAsync(Guid loanId, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
        if (loan is null)
            return Result<GetInstallmentsResultDto>.Failure(new Error(loan.LastErrorCode, "درخواست وام یافت نشد."));

        //var res = await _mediator.Send(new GetInstallmentsQuery
        //{
        //    NationalCode = loan.Customer.NationalCode,
        //    ContractNumber = loan.Contract.ApprovalCode
        //}, ct);
        var res = await _mediator.Send(new GetInstallmentsQuery
        {
            NationalCode = loan.Customer.NationalCode
            //ContractNumber = loan.Contract.ApprovalCode
        }, ct);
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<GetInstallmentsResultDto>(providerType, "Installments");

        var decision = policy.Evaluate(res);
        var d = decision.Value!;
        if (!decision.IsSuccess)
        {

            if (decision.Value?.Retryable == true)
            {
                loan.TransitionTo(LoanRequestState.Failed, decision.Value.ReasonCode, decision.Value.UiMessage);
                await _repo.UpdateAsync(loan, ct);

                _jobs.EnqueueRetryGetInstallment(loanId, TimeSpan.FromSeconds(45), ct);
            }
            return await FailAndReturn<GetInstallmentsResultDto>(loan, decision.Error!, ct);
        }

        if (res.Installments is not null && res.Installments.Any())
        {
            var installment = _mapper.Map<List<InstallmentStatus>>(res.Installments);

            installment.FirstOrDefault().ContractNumber = res.ContractNumber;

            _repo.InsertInstallmentAsync(installment, loanId, ct);
        }

        return Result<GetInstallmentsResultDto>.Success(new GetInstallmentsResultDto
        {
            RequestId = res.RequestId,
            State = loan.State.ToString(),
            Installments = res.Installments,
            Message = decision.Value!.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }

    /// <summary>
    /// مانده اعتبار قرارداد
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<GetCustomerCreditBalanceResultDto>> GetCreditBalanceAsync(Guid loanId, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
        var res = await _mediator.Send(new GetCustomerCreditBalanceCommand
        {
            NationalCode = loan.Customer.NationalCode!
            //ContractNumber = Convert.ToDecimal(loan.Contract.ContractNumber)
        }, ct);
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<GetCustomerCreditBalanceResultDto>(providerType, "CreditBalance");

        var decision = policy.Evaluate(res);

        if (!decision.IsSuccess)
        {
            if(decision.Value.Retryable==true)
            {
                loan.TransitionTo(LoanRequestState.Failed, decision.Value.ReasonCode, decision.Value.UiMessage);
                await _repo.UpdateAsync(loan, ct);
                _jobs.EnqueueRetryCreditBalance(loanId,  TimeSpan.FromSeconds(45), ct);

                return Result<GetCustomerCreditBalanceResultDto>.Failure(
                                   new Error(decision.Value.ReasonCode, "در حال تلاش مجدد برای دریافت موجودی اعتبار"));
            }
            return await FailAndReturn<GetCustomerCreditBalanceResultDto>(loan, decision.Error!, ct);
        }

        return Result<GetCustomerCreditBalanceResultDto>.Success(new GetCustomerCreditBalanceResultDto
        {
            RequestId = res.RequestId,
            ContractCreditList = res.ContractCreditList,
            State = loan.State.ToString(),
            Message = decision.Value!.UiMessage,
            NextActions = GetNextActions(loan)
        });
    }

    // ---------------- 12) صورتحساب و 13) ریزخریدها --------------
    public async Task<Result<GetCustomerBillingResultDto>> GetBillingAsync(Guid loanId, GetCustomerBillingCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
        var res = await _mediator.Send(cmd with { /*ContractNumber =loan.Contract.ContractNumber,*/ NationalCode = loan.Customer.NationalCode! }, ct);
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

    /// <summary>
    /// سرو ی س دریافت فهرست ریزخریدها )برای قراردادهای اعتبار در خرید(
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<GetCustomerPurchaseDetailsResultDto>> GetPurchasesAsync(Guid loanId, GetCustomerPurchaseDetailsCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
    
        
        var providerType = loan.Provider.ProviderType;
        var res = await _mediator.Send(cmd with {
            //ContractNumber = loan.Contract.ContractNumber,
            NationalCode = loan.Customer.NationalCode!,
            ProviderType=providerType
          }, ct);
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

    // ---------------- 14) ثبت حواله و 15) استعلام حواله --------------
    public async Task<Result<TransferRegisterResultDto>> TransferRegisterAsync(Guid loanId, TransferRegisterCommand cmd, CancellationToken ct)
    {
        var loan = await RequireAsync(loanId, ct);
        var res = await _mediator.Send(cmd, ct);
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<TransferRegisterResultDto>(providerType, "TransferRegister");
        var decision = policy.Evaluate(res);
        if (!decision.IsSuccess)
            return await FailAndReturn<TransferRegisterResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;
        loan.MarkRemittanceRegistering(d.ReasonCode, d.UiMessage);
        loan.SetRegisterCode(res.RegisterCode);
        await _repo.UpdateAsync(loan, ct);


        //  await _jobs.EnqueueTransferInquiryAsync(loan.Id, res.RegisterCode, TimeSpan.FromSeconds(45), ct);

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
        var res = await _mediator.Send(new TransferInquiryQuery { RegisterCode = loan.Transfer.RegisterCode! }, ct);
        var providerType = loan.Provider.ProviderType;

        var policy = _bankPolicyFactory.CreatePolicy<TransferInquiryResultDto>(providerType, "TransferInquiry");

        var decision = policy.Evaluate(res);
        if (!decision.IsSuccess)
            return await FailAndReturn<TransferInquiryResultDto>(loan, decision.Error!, ct);

        var d = decision.Value!;
        if (res.InquiryDetails.FirstOrDefault().transferStatus == TransferInquiryResultDto.TransferStatus.Registered)
            loan.MarkDisbursed(d.ReasonCode, d.UiMessage);
        // برگشتی/لغو و برگشت
        else if (res.InquiryDetails.FirstOrDefault().transferStatus == TransferInquiryResultDto.TransferStatus.Returned ||
            res.InquiryDetails.FirstOrDefault().transferStatus == TransferInquiryResultDto.TransferStatus.CanceledAndReturned)
            loan.MarkRemittanceReturned(d.ReasonCode, d.UiMessage);
        // عدم امکان ارسال
        else if (res.InquiryDetails.FirstOrDefault().transferStatus == TransferInquiryResultDto.TransferStatus.Unsendable)
            loan.MarkRemittanceFailed(d.ReasonCode, d.UiMessage);

        else loan.MarkRemittancePending(d.ReasonCode, d.UiMessage);

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
        => await _repo.GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Loan request not found.");

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








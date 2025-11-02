using LoanService.Domain.Enum;

namespace LoanService.Domain.Entities;

//public sealed class LoanRequest1
//{

//    public Guid Id { get; private set; }
//    public string RequestId { get; private set; } = default!;
//    public string NationalCode { get; private set; } = default!;
//    public DateTime? BirthDate { get; private set; }
//    public string? MobileNo { get; private set; }
//    public string? PostalCode { get; private set; }
//    public decimal? RequestAmount { get; private set; }
//    public decimal? ApprovalCode { get; private set; }
//    public decimal? CbTrackingCode { get; private set; }

//    public LoanRequestState State { get; private set; } 

//    public decimal? MaxApprovedAmount { get; private set; }
//    public bool Allowed { get; private set; } = false;
//    public int? Ics { get; private set; }
//    public string? IcsGrade { get; private set; }
//    public int? Gender { get; private set; } // 0=Male, 1=Female
//    public DateTime? RequestExpireDate { get; private set; }

//    public string? MessageCode { get; private set; }
//    public string? Message { get; private set; }

//    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
//    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;


//    //----------PayResponse-----------------

//    public decimal? ApprovedLoanAmount { get; private set; }
//    public string? ContractDate { get; private set; }
//    public decimal? CentralBankTraceCode { get; private set; }
//    public string? BankSignedContractBase64 { get; private set; }
//    public DateTime? PayResponseReceivedAtUtc { get; private set; }

//    // -------- Core Identifiers --------
   
//    public BankProviderType Provider { get; private set; } 
//    public string ProductCode { get; private set; } = default!;


//    // -------- Customer / Context --------

//    public string? Mobile { get; private set; }
//    public string? CustomerAccountNo { get; private set; }
//    public string? CorrelationId { get; private set; }

//    // -------- Inquiry --------

//    public string? InquiryRequestId { get; private set; }





//    // -------- Contract / Facility --------
//        // از مصوبه/مستند
//    public string? ContractId { get; private set; }              // شماره قرارداد بانک
//    public string? ContractDesc { get; private set; }
//    public string? PayRequestId { get; private set; }            // شماره پیگیری ثبت اعطا
//    public string? RequestedAmount { get; private set; }            // شماره پیگیری ثبت اعطا
//    public string? SignedContractBase64 { get; private set; }            // شماره پیگیری ثبت اعطا


//    // -------- Remittance / Transfer --------
//    public string? RegisterCode { get; private set; }            // شماره پیگیری حواله
//    public decimal? TransactionNumber { get; private set; }      // خروجی DepositRequest

//    // -------- Error / Reason --------
//    public string? LastReasonCode { get; private set; }
//    public string? LastReasonMessage { get; private set; }
//    public string? LastErrorCode { get; private set; }
//    public string? LastErrorMessage { get; private set; }

//    // -------- Audit --------
//    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
//    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;
//    public bool RequiresOtp { get; private set; }//appsettingتنظیمش کن 
//    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

//    // ===================== Factory =====================
//    public static LoanRequest1 Create( string nationalCode)
//    {
//        return new LoanRequest1
//        {
           
//            NationalCode = nationalCode,
//            //Mobile = mobile,
//            //ApprovalCode = approvalCode,
//            //CustomerAccountNo = customerAccountNo,
//            //CorrelationId = correlationId
//        };
//    }

//    /// <summary>
//    /// آیا محصول از نوع اعتبار در خرید است؟
//    /// مثلاً ProductCodeهایی که با "PC" یا "CREDIT" شروع می‌کنند.
//    /// </summary>
//    public bool IsPurchaseCredit =>
//        !string.IsNullOrWhiteSpace(ApprovalCode.ToString()) &&
//        (ApprovalCode.ToString().StartsWith("PC", StringComparison.OrdinalIgnoreCase)
//         || ApprovalCode.ToString().Contains("PURCHASE", StringComparison.OrdinalIgnoreCase)
//         || ApprovalCode.ToString().Contains("CREDIT", StringComparison.OrdinalIgnoreCase));

//    public string? LastRepaymentTrackNumber { get; private set; }
//    public string? LastRepaymentAccountNo { get; private set; }
//    public decimal? LastRepaymentAmount { get; private set; }
//    public DateTime LastRepaymentAtUtc { get; private set; }

//    // ===================== Generic Transition =====================
//    public void TransitionTo(LoanRequestState next, string? reasonCode, string? uiMessage)
//    {
//        State = next;
//        LastReasonCode = reasonCode;
//        LastReasonMessage = uiMessage;
//        Touch();
//    }

//    // ===================== Inquiry helpers =====================
//    public void SetInquiryRequestId(string requestId)
//    {
//        InquiryRequestId = requestId;
//        Touch();
//    }

//    public void SetInquiryDecision(bool allowed, decimal? maxApproved, int? ics = null,
//                                   string? icsGrade = null, DateTime? expire = null)
//    {
//        Allowed = allowed;
//        MaxApprovedAmount = maxApproved;
//        Ics = ics;
//        IcsGrade = icsGrade;
//        RequestExpireDate = expire;
//        Touch();
//    }

//    // ===================== Contract helpers =====================
//    public void AttachContract(string contractNumber, string? reasonCode, string? uiMessage, string? desc = null)
//    {
//        ContractId = contractNumber;
//        ContractDesc = desc;
//        TransitionTo(LoanRequestState.ContractsPrepared, reasonCode, uiMessage);
//    }

//    public void MarkFacilitySubmitted(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.FacilitySubmitted, reasonCode, uiMessage);

//    public void MarkApproved(string? reasonCode, string? uiMessage, string? contractNumber = null)
//    {
//        if (!string.IsNullOrWhiteSpace(contractNumber))
//            ContractId = contractNumber;
//        TransitionTo(LoanRequestState.Approved, reasonCode, uiMessage);
//    }

//    public void MarkUnderReview(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.UnderReview, reasonCode, uiMessage);

//    public void MarkRejected(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.Rejected, reasonCode, uiMessage);

//    public void SetPayRequestId(string payRequestId)
//    {
//        PayRequestId = payRequestId;
//        Touch();
//    }

//    // ===================== OTP / Deposit =====================
//    public void MarkOtpSent(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.OtpSent, reasonCode, uiMessage);

//    public void MarkOtpVerified(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.OtpVerified, reasonCode, uiMessage);

//    public void SetTransactionNumber(decimal txNo)
//    {
//        TransactionNumber = txNo;
//        Touch();
//    }

//    // ===================== Transfer / Remittance =====================
//    public void SetRegisterCode(string registerCode)
//    {
//        RegisterCode = registerCode;
//        Touch();
//    }

//    public void MarkRemittanceRegistering(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.RemittancePending, reasonCode, uiMessage);

//    public void MarkRemittancePending(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.RemittancePending, reasonCode, uiMessage);

//    public void MarkRemittanceReturned(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.RemittanceReturned, reasonCode, uiMessage);

//    public void MarkRemittanceFailed(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.Failed, reasonCode, uiMessage);

//    public void MarkDisbursed(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.Disbursed, reasonCode, uiMessage);

//    // ===================== Repayment =====================
//    public void MarkRepaymentRegistered(string? reasonCode, string? uiMessage)
//        => TransitionTo(LoanRequestState.RepaymentRegistered, reasonCode, uiMessage);

//    // ===================== Errors =====================
//    public void MarkTemporaryFailure(string? code, string? message)
//    {
//        LastErrorCode = code;
//        LastErrorMessage = message;
//        TransitionTo(LoanRequestState.Failed, code, message);
//    }

//    public void MarkIneligible(string? code, string? message)
//    {
//        LastErrorCode = code;
//        LastErrorMessage = message;
//        TransitionTo(LoanRequestState.Ineligible, code, message);
//    }

//    public void MarkKycChecked(string? code, string? message)
//        => TransitionTo(LoanRequestState.KYCChecked, code, message);

//    // ===================== Mutators (اختیاری برای به‌روزرسانی داده‌های پایه) =====================
//    public void UpdateCustomer(string? nationalCode, string? mobile, string? accountNo)
//    {
//        NationalCode = nationalCode ?? NationalCode;
//        Mobile = mobile ?? Mobile;
//        CustomerAccountNo = accountNo ?? CustomerAccountNo;
//        Touch();
//    }
//    public void SetLastRepaymentInfo(string? trackNumber, 
//        string? accountNo, 
//        decimal? amount,
//        DateTime? whenUtc)
//    {
//        LastRepaymentTrackNumber = trackNumber;
//        LastRepaymentAccountNo = accountNo;
//        LastRepaymentAmount = amount;
//        LastRepaymentAtUtc = whenUtc ?? DateTime.UtcNow;
//        Touch();
//    }
//    public void SetApprovalCode(decimal? approvalCode)
//    {
//        ApprovalCode = approvalCode;
//        Touch();
//    }
//    public void SetPayResponseApprovedInfo(
//    string? contractNo,
//    decimal? loanAmount,
//    string? contractDate,
//    decimal? traceCode = null,
//    string? bankSignedContractBase64 = null)
//    {
    
//        if (string.IsNullOrWhiteSpace(contractNo))
//            throw new ArgumentException("contractNo is required for approved pay response.", nameof(contractNo));

  
//        if (State is not LoanRequestState.FacilitySubmitted and not LoanRequestState.Approved)
//            throw new InvalidOperationException($"Cannot set approved pay info in state {State}.");

//        ContractId = contractNo;
//        ApprovedLoanAmount = loanAmount;
//        ContractDate = contractDate;

//        if (traceCode>0)
//            CentralBankTraceCode = traceCode;

//        if (!string.IsNullOrWhiteSpace(bankSignedContractBase64))
//            BankSignedContractBase64 = bankSignedContractBase64;

//        PayResponseReceivedAtUtc = DateTime.UtcNow;
//        Touch(); // به‌روزرسانی UpdatedAtUtc یا هر متد داخلی مشابه
//    }
//}







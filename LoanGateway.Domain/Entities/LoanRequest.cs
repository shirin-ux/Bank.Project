using LoanService.Domain.Enum;
using LoanService.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities
{
    public sealed class LoanRequest // : IAggregateRoot (در صورت داشتن مارکر)
    {
        // -------- Keys / Timestamps --------
        public Guid Id { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

        // -------- Core --------
        public LoanRequestState State { get; private set; } = LoanRequestState.Requested;
        public ProviderInfo Provider { get; private set; } = default!;
        public CustomerInfo Customer { get; private set; } = default!;
        public bool RequiresOtp { get; private set; }

        public TimeSpan RowVersion { get; set; }
        public Guid CorrelationId { get; set; }
        // -------- Slices --------
     
        public InqueryRequest InqueryRequest { get; private set; }
        public GrantRequest GrantRequest { get; private set; }
        public InquiryInfo Inquiry { get; private set; } = new( null, null, null, null, null);
        public ContractInfo Contract { get; private set; }
        public PayRequestInfo PayRequest { get; private set; } = new(null, null);
        public PayResponseInfo PayResponse { get; private set; } 
        public TransferInfo Transfer { get; private set; } = new(null, null);
        public RepaymentSnapshot LastRepayment { get; private set; } = new(null, null, null, default);

        public InstallmentStatus InstallmentStatus { get; private set; }

        // -------- Last Decision / Errors --------
        public DecisionStamp LastDecision { get; private set; } = new(null, null, null, null);

        // -------- Convenience flags --------
        public bool IsPurchaseCredit =>
        Provider.ApprovalCode <= 0;

        private void Touch() => UpdatedAtUtc = DateTime.UtcNow;


      // -------- Error / Reason --------
        public string? LastReasonCode { get; private set; }
        public string? LastReasonMessage { get; private set; }
        public string? LastErrorCode { get; private set; }
        public string? LastErrorMessage { get; private set; }


        // ===================== Factory =====================
        public static LoanRequest Create(string nationalCode, BankProviderType provider, decimal? ApprovalCode, bool requiresOtp)
        {
            return new LoanRequest
            {
                Id = Guid.NewGuid(),
                Customer = new CustomerInfo(nationalCode, null, null, null, null),
                Provider = new ProviderInfo(provider, ApprovalCode, requiresOtp),
                State = LoanRequestState.Requested
            }.TouchReturn();
        }
        private LoanRequest TouchReturn() { Touch(); return this; }

        // ===================== Transitions (domain behavior) =====================

        // --- Inquiry ---
        public void MarkKycChecked(string reasonCode, string uiMessage, string requestId)
        {
            InqueryRequest = InqueryRequest with { RequestId = requestId };
            TransitionTo(LoanRequestState.KYCChecked, reasonCode, uiMessage);
        }

        public void ApplyInquiryDecision(
            bool allowed, decimal? maxApproved,
            int? ics = null, Grade? icsGrade = null, DateTime? expire = null,
            string reasonCode = "INQ.DECISION", string? uiMessage = null)
        {
            Inquiry = new InquiryInfo(allowed, maxApproved, ics, icsGrade, expire);
            TransitionTo(allowed ? LoanRequestState.Eligible : LoanRequestState.Ineligible, reasonCode, uiMessage);
        }

        public void AttachContract(decimal contractNumber, string reasonCode, string uiMessage, string? desc = null, bool withCollateral = false)
        {
            if (contractNumber<=0)
                throw new ArgumentException("contractNumber is required.", nameof(contractNumber));

    
            Contract = Contract with { ContractNumber = contractNumber, Desc = desc, WithCollateral = withCollateral };

            // تغییر وضعیت دامین
            TransitionTo(LoanRequestState.ContractsPrepared, reasonCode, uiMessage);
        }
        public void MarkApproved(string? reasonCode, string? uiMessage, decimal contractNumber )
        {
            if (contractNumber>0)
                GrantRequest = GrantRequest with { ContractId = contractNumber };

            TransitionTo(LoanRequestState.Approved, reasonCode, uiMessage);
        }
        public void MarkRemittanceRegistering(string? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.RemittancePending, reasonCode, uiMessage);
        public void SetTransactionNumber(decimal txNo)
        {
           Transfer=Transfer with { TransactionNumber = txNo };
            Touch();
        }

        public void SetPayResponseApprovedInfo(
           decimal contractNo,
           decimal? loanAmount,
           string? contractDate,
           decimal? traceCode = null,
           string? bankSignedContractBase64 = null)
        {

            if (contractNo <= 0)
                throw new ArgumentException("contractNo is required for approved pay response.", nameof(contractNo));


            if (State is not LoanRequestState.FacilitySubmitted and not LoanRequestState.Approved)
                throw new InvalidOperationException($"Cannot set approved pay info in state {State}.");

           PayResponse=PayResponse with { BankContractNo = contractNo, ApprovedLoanAmount = loanAmount, ContractDate = contractDate };

            if (traceCode > 0)
                PayResponse = PayResponse with { CentralBankTraceCode = traceCode };

            if (!string.IsNullOrWhiteSpace(bankSignedContractBase64))
                PayResponse = PayResponse with { BankSignedContractBase64 = bankSignedContractBase64 };

            PayResponse = PayResponse with { ReceivedAtUtc = DateTime.UtcNow };
            ;
            Touch(); // به‌روزرسانی UpdatedAtUtc یا هر متد داخلی مشابه
        }
        

        public void MarkFacilitySubmitted(string? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.FacilitySubmitted, reasonCode, uiMessage);

        // --- Contract ---
        public void AttachContract(decimal contractNumber, string reasonCode, string uiMessage, string? desc = null)
        {
            Contract = Contract with { ContractNumber = contractNumber, Desc = desc };
            TransitionTo(LoanRequestState.ContractsPrepared, reasonCode, uiMessage);
        }

        public void MarkUnderReview(string? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.UnderReview, reasonCode, uiMessage);

        public void MarkRejected(string? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.Rejected, reasonCode, uiMessage);

        public void SetApprovalCode(string? approvalCode)
        {
            Contract = Contract with { ApprovalCode = approvalCode };
            Touch();
        }
        public void SetOtpRequirement(bool requiresOtp)
        {
            RequiresOtp = requiresOtp;
            Touch();
        }
        public void SaveSignedContract(string base64)
        {
            Contract = Contract with { SignedContractBase64 = base64 };
            Touch();
        }
        public void SetPayRequestId(string payRequestId)
        {
          PayRequest=PayRequest with { PayRequestId = payRequestId };
            Touch();
        }
        public void MarkFacilitySubmitted(string reasonCode, string uiMessage, string payRequestId)
        {
            PayRequest = PayRequest with { PayRequestId = payRequestId };
            TransitionTo(LoanRequestState.FacilitySubmitted, reasonCode, uiMessage);
        }
        public void MarkRemittanceFailed(string? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.Failed, reasonCode, uiMessage);
        // --- Pay Response ---
        public void ApplyPayResponse(
            PayResponseCode code,
            decimal bankContractNo,
            decimal? approvedAmount,
            string? contractDate,
            decimal? centralBankTraceCode,
            string? bankSignedBase64,
            string reasonCode,
            string? uiMessage)
        {
            PayResponse = new PayResponseInfo(code, bankContractNo, approvedAmount, contractDate, centralBankTraceCode, bankSignedBase64, DateTime.UtcNow);

            switch (code)
            {
                case PayResponseCode.Success:
                    // به‌روزرسانی قرارداد بانک
                    if (bankContractNo<=0)
                        Contract = Contract with { ContractNumber = bankContractNo };
                    TransitionTo(LoanRequestState.Approved, reasonCode, uiMessage);
                    break;
                case PayResponseCode.Pending:
                    TransitionTo(LoanRequestState.UnderReview, reasonCode, uiMessage);
                    break;
                case PayResponseCode.Failed:
                    TransitionTo(LoanRequestState.Rejected, reasonCode, uiMessage);
                    break;
                case PayResponseCode.NotAllowed:
                    TransitionTo(LoanRequestState.Ineligible, reasonCode, uiMessage);
                    break;
                default:
                    TransitionTo(LoanRequestState.Failed, reasonCode, uiMessage ?? "Invalid pay response.");
                    break;
            }
        }
  

        // --- OTP ---
        public void MarkOtpSent(string reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.OtpSent, reasonCode, uiMessage);

        public void MarkOtpVerified(string reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.OtpVerified, reasonCode, uiMessage);

        // --- Deposit / Remittance ---
        public void RegisterDeposit(string reasonCode, string uiMessage, decimal transactionNumber)
        {
            Transfer = Transfer with { TransactionNumber = transactionNumber };
            TransitionTo(LoanRequestState.RemittancePending, reasonCode, uiMessage);
        }

        public void SetRegisterCode(string registerCode)
        {
            Transfer = Transfer with { RegisterCode = registerCode };
            Touch();
        }

        public void MarkRemittancePending(string reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.RemittancePending, reasonCode, uiMessage);

        public void MarkRemittanceReturned(string reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.RemittanceReturned, reasonCode, uiMessage);

        public void MarkDisbursed(string reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.Disbursed, reasonCode, uiMessage);


        public bool CanPerformRepayment(string? otpCode)
        {
            if (RequiresOtp && string.IsNullOrWhiteSpace(otpCode))
                return false;
            return true;
        }
     

        //    public void TransitionTo(LoanRequestState next, string? reasonCode, string? uiMessage)
        //    {
        //        State = next;
        //        LastReasonCode = reasonCode;
        //        LastReasonMessage = uiMessage;
        //        Touch();
        //    }

        // --- Repayment ---
        public void MarkRepaymentRegistered(string reasonCode, string uiMessage, string? trackNumber, string? accountNo, decimal? amount, DateTime? whenUtc)
        {
            LastRepayment = new RepaymentSnapshot(trackNumber, accountNo, amount, whenUtc ?? DateTime.UtcNow);
            TransitionTo(LoanRequestState.RepaymentRegistered, reasonCode, uiMessage);
        }
        //public void SetLastRepaymentInfo(string? trackNumber,
        //    string? accountNo,
        //    decimal? amount,
        //    DateTime? whenUtc)
        //{
        //    LastRepayment.TrackNumber= trackNumber;
        //    LastRepaymentAccountNo = accountNo;
        //    LastRepaymentAmount = amount;
        //    LastRepaymentAtUtc = whenUtc ?? DateTime.UtcNow;
        //    Touch();
        //}
        // --- Errors / decision ---
        public void MarkIneligible(string? code, string? message)
        {
            LastDecision = LastDecision with { ErrorCode = code, ErrorMessage = message, ReasonCode = code, ReasonMessage = message };
            TransitionTo(LoanRequestState.Ineligible, code ?? "INELIGIBLE", message);
        }

        public void MarkTemporaryFailure(string? code, string? message)
        {
            LastDecision = LastDecision with { ErrorCode = code, ErrorMessage = message, ReasonCode = code, ReasonMessage = message };
            TransitionTo(LoanRequestState.Failed, code ?? "TEMP.FAIL", message);
        }

        // --- Generic transition (keeps last decision) ---
        public void TransitionTo(LoanRequestState next, string? reasonCode, string? uiMessage)
        {
            State = next;
            LastReasonCode = reasonCode;
            LastReasonMessage = uiMessage;
            Touch();
        }


        // ===================== Inquiry helpers =====================
        public void SetInquiryRequestId(string requestId)
        {
            InqueryRequest =  InqueryRequest with{ RequestId = requestId };
            Touch();
        }
        public void SetInquiryDecision(bool? allowed, decimal? maxApproved, int? ics = null,
                                       Grade? icsGrade = null, DateTime? expire = null)
        {
            Inquiry = new InquiryInfo(allowed, maxApproved, ics, icsGrade, expire);
            Touch();
        }
        // --- Mutators (اختیاری) ---
        public void UpdateCustomer(string? mobile = null, string? postalCode = null, DateTime? birthDate = null, int? gender = null)
        {
            Customer = new CustomerInfo(Customer.NationalCode, birthDate ?? Customer.BirthDate, mobile ?? Customer.Mobile, postalCode ?? Customer.PostalCode, gender ?? Customer.Gender);
            Touch();
        }
    }
    public enum PayResponseCode
    {
        Unknown = 0,
        Pending = 1,
        Success = 2,
        Failed = 3,
        NotAllowed = 4
    }
    public enum LoanRequestState
    {
        /// <summary>
        /// درخواست وام ثبت شده ولی هنوز هیچ اقدامی انجام نشده است.
        /// </summary>
        Requested = 0,

        /// <summary>
        /// اطلاعات مشتری جهت استعلام به بانک ارسال شده است.
        /// (در انتظار پاسخ از بانک یا مصوبه لندتک)
        /// </summary>
        KYCChecked = 1,

        /// <summary>
        /// مشتری واجد شرایط دریافت تسهیلات شناخته شده است.
        /// (Eligibility تایید شده)
        /// </summary>
        Eligible = 2,

        /// <summary>
        /// مشتری واجد شرایط نیست.
        /// (مثلاً بدهی بانکی یا خطای احراز هویت)
        /// </summary>
        Ineligible = 3,

        /// <summary>
        /// قراردادهای مشتری (با یا بدون وثیقه) تولید شده‌اند و آماده‌ی امضا هستند.
        /// </summary>
        ContractsPrepared = 4,

        /// <summary>
        /// کد تأیید (OTP) برای کاربر ارسال شده است.
        /// </summary>
        OtpSent = 5,

        /// <summary>
        /// کد تأیید (OTP) با موفقیت تأیید شده است.
        /// </summary>
        OtpVerified = 6,

        /// <summary>
        /// درخواست تسهیلات (Facility) یا پرداخت (PayRequest) به بانک ارسال شده است.
        /// </summary>
        FacilitySubmitted = 7,

        /// <summary>
        /// درخواست در حال بررسی یا اعتبارسنجی نهایی توسط بانک است.
        /// </summary>
        UnderReview = 8,

        /// <summary>
        /// درخواست وام/تسهیلات توسط بانک تایید شده است.
        /// </summary>
        Approved = 9,

        /// <summary>
        /// درخواست توسط بانک یا سیاست‌های لندتک رد شده است.
        /// </summary>
        Rejected = 10,

        /// <summary>
        /// حواله در حال ثبت است.
        /// </summary>
        RemittanceRegistering = 11,

        /// <summary>
        /// حواله ثبت شده ولی هنوز واریز نهایی انجام نشده است.
        /// </summary>
        RemittancePending = 12,

        /// <summary>
        /// وجه تسهیلات با موفقیت واریز شده است.
        /// </summary>
        Disbursed = 13,

        /// <summary>
        /// قرارداد فعال است و اقساط در حال بازپرداخت می‌باشند.
        /// </summary>
        Active = 14,

        /// <summary>
        /// قرارداد تسویه یا بسته شده است.
        /// </summary>
        Completed = 15,

        /// <summary>
        /// خطای موقت یا سیستمی رخ داده است (قابل retry).
        /// </summary>
        Failed = 16,

        /// <summary>
        /// درخواست نیاز به بررسی دستی دارد (مثلاً مغایرت اطلاعات).
        /// </summary>
        ManualReview = 17,

        RepaymentRegistered = 18,

        RemittanceReturned = 19,

        Unknown = 20,
        CreditChecked = 21

    }
    public enum DepositType : short
    {
        AnyAccount = 1,     // واریز به هر حساب
        FixedAccount = 2,   // واریز به حساب ثابت (مصوبه)
        CustomerAccount = 3 // واریز به حساب مشتری
    }
}

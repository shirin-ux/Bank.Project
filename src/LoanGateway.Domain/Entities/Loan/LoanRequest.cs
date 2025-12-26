using LoanService.Domain.Enum.Loan;
using LoanService.Domain.Enum;
using LoanService.Domain.ValueObjects;
using System.Net;
using static LoanService.Domain.Entities.Loan.InquiryInfo;

namespace LoanService.Domain.Entities.Loan
{
    public sealed class LoanRequest :BaseEntity
    {
  

        // -------- Core --------
        public LoanRequestState State { get;  set; } 
        public ProviderInfo Provider { get; set; } = default!;
        public CustomerInfo? Customer { get; set; } = default!;
        public bool RequiresOtp { get; private set; }
        public decimal? RequestAmount { get; private set; }
        public bool RequiresCollateral { get; private set; }
        public CollateralType CollateralType { get; private set; } = CollateralType.Unknown;
        public Guid CorrelationId { get; set; }
        // -------- Slices --------

        public int RetryCount { get; set; }
        public string? InquiryRequest_Id { get; set; }
        public InqueryRequest InqueryRequest { get; set; } = new(null);
        public GrantRequest? GrantRequest { get; set; } = default!;
        public InquiryInfo Inquiry { get; set; } = default!;
        public ContractInfo Contract { get; set; } = default!;
        public PayRequestInfo PayRequest { get; set; } = new(null, null);
        public PayResponseInfo PayResponse { get; set; }
        public TransferInfo Transfer { get; set; } = default!;
        public RepaymentSnapshot LastRepayment { get; set; } = default!;

        public InstallmentStatus InstallmentStatus { get; set; } = default!;

        // -------- Last Decision / Errors --------
        public DecisionStamp LastDecision { get; set; } = new(null, null, null, null);

        // -------- Convenience flags --------
        public bool IsPurchaseCredit =>
        Provider.ApprovalCode <= 0;




        // -------- Error / Reason --------
        public int? LastReasonCode { get; private set; }
        public string? LastReasonMessage { get; private set; }
        public int? LastErrorCode { get; private set; }
        public string? LastErrorMessage { get; private set; }


        // ===================== Factory =====================


        public static LoanRequest Create(string nationalCode,
            string? birthDate, 
            string? postalCode,
            string? mobileNo,
            ProviderType providerType,
            decimal? ApprovalCode,
            bool requiresOtp)
        {
            return new LoanRequest
            {
                Id = Guid.NewGuid(),
                Customer = new CustomerInfo(nationalCode, birthDate, mobileNo, postalCode, null),
                Provider = new ProviderInfo(providerType, ApprovalCode, requiresOtp),
                State = LoanRequestState.Requested,
                InqueryRequest = new InqueryRequest(null),
                GrantRequest = new GrantRequest(null, null, null, null, null),
                PayRequest = new PayRequestInfo(null, null),
                LastDecision = new DecisionStamp(null, null, null, null)
            }.TouchReturn();
        }
        public bool TryIncreasePayResponseRetry(int max)
        {
            if (RetryCount >= max)
                return false;

            RetryCount++;
            Touch();
            return true;
        }
        public void SetRequest(decimal? requestAmount, bool requiresCollateral, CollateralType collateralType)
        {
            if (requestAmount.HasValue)
            {
                requiresCollateral = requestAmount.Value > 20_000_000;
                collateralType = requiresCollateral ? CollateralType.PROMISSORY : CollateralType.Unknown;
            }
            RequestAmount = requestAmount;
            RequiresCollateral = requiresCollateral;
            CollateralType = collateralType;
        }
        public void SetCollateralType(CollateralType type)
        {
            CollateralType = type;
            Touch();
        }
        private LoanRequest TouchReturn() { Touch(); return this; }

        // ===================== Transitions (domain behavior) =====================

        // --- Inquiry ---
        public void MarkKycChecked(int reasonCode, string uiMessage, string requestId)
        {
            InqueryRequest = InqueryRequest with { RequestId = requestId };
            TransitionTo(LoanRequestState.KYCChecked, reasonCode, uiMessage);
        }

        public void ApplyInquiryDecision(
            bool allowed, decimal? maxApproved,
            int? ics = null, Grade? icsGrade = null, string? expire = null,
            int reasonCode = -1, string? uiMessage = null)
        {
            Inquiry = new InquiryInfo
            {
                Allowed = allowed,
                ExpireAt = expire,
                Ics = ics,
                IcsGrade = icsGrade,
                MaxApprovedAmount = maxApproved
            };
            TransitionTo(allowed ? LoanRequestState.Eligible : LoanRequestState.Ineligible, reasonCode, uiMessage);
        }

        public void AttachContract(
            decimal? approvalCode,
            string address,
            DateTime birthDate,
            string nationalCode,
            short installmentCount,
            decimal? loanAmount,
            string mobileNumber,
            string? phoneNumber,
            string postalCode,
            string contractPath,
            string contractNumber,
             int reasonCode = -1, string? uiMessage = null
            )
        {
            //if (Provider?.ApprovalCode == null || Provider.ApprovalCode <= 0)
            //    throw new InvalidOperationException("ApprovalCode must be assigned before attaching a contract.");


            Contract = new ContractInfo
            {
                ApprovalCode = approvalCode,
                Address = address,
                BirthDate = birthDate,
                NationalCode = nationalCode,
                InstallmentCount = installmentCount,
                LoanAmount = loanAmount,
                MobileNumber = mobileNumber,
                PhoneNumber = phoneNumber,
                PostalCode = postalCode,
                ContractPath= contractPath,
                ContractNumber= contractNumber,
            };

            // تغییر وضعیت دامین
            TransitionTo(LoanRequestState.ContractsPrepared, reasonCode, uiMessage);
        }
        public void AttachCollateralContract(decimal collateralNo,
            CollateralType collateralType, 
            string CollateralDate,
            decimal? approvalCode,
            decimal collateralAmount,
            string address,
            DateTime birthDate,
            string? chequeSerial,
            string? collateralIssuer,
            string guarantorNC,
            string nationalCode,
            short installmentCount,
            decimal? loanAmount,
            string mobileNumber,
            string? phoneNumber,
            string postalCode,
            string contractPath,
            string contractNumber,
             int reasonCode = -1, string? uiMessage = null
                                              )
        {
            if (collateralNo <= 0)
                throw new ArgumentException("collateralNo is required.", nameof(collateralNo));


            Contract = new ContractInfo
            {
                CollateralNo = collateralNo,
                CollateralType = collateralType,
                CollateralDate = CollateralDate,
                ApprovalCode = approvalCode,
                CollateralAmount = collateralAmount,
                Address = address,
                BirthDate = birthDate,
                ChequeSerial = chequeSerial,
                CollateralIssuer = collateralIssuer,
                GuarantorNC = guarantorNC,
                NationalCode = nationalCode,
                InstallmentCount = installmentCount,
                LoanAmount = loanAmount,
                MobileNumber = mobileNumber,
                PhoneNumber = phoneNumber,
                PostalCode = postalCode,
                ContractPath= contractPath,
               ContractNumber = contractNumber,
            };

            // تغییر وضعیت دامین
            TransitionTo(LoanRequestState.ContractsPrepared, reasonCode, uiMessage);
        }
        public void MarkApproved(int? reasonCode, string? uiMessage, decimal? contractNumber)
        {
            if (contractNumber > 0)
                GrantRequest = GrantRequest with { ContractId = contractNumber };

            TransitionTo(LoanRequestState.Approved, reasonCode, uiMessage);
        }
        public void MarkRemittanceRegistering(int? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.RemittancePending, reasonCode, uiMessage);
        public void SetTransactionNumber(decimal txNo)
        {
            Transfer = new TransferInfo { TransactionNumber = txNo };
            Touch();
        }

        public void SetPayResponseApprovedInfo(
           decimal? contractNo,
           decimal? loanAmount,
           string? contractDate,
           decimal? traceCode = null,
           string? bankSignedContractBase64 = null)
        {

            if (contractNo <= 0)
                throw new ArgumentException("contractNo is required for approved pay response.", nameof(contractNo));


            if (State is not LoanRequestState.FacilitySubmitted and not LoanRequestState.Approved)
                throw new InvalidOperationException($"Cannot set approved pay info in state {State}.");

            PayResponse = new PayResponseInfo { BankContractNo = contractNo, ApprovedLoanAmount = loanAmount, ContractDate = contractDate };

            if (traceCode > 0)
                PayResponse = new PayResponseInfo { CentralBankTraceCode = traceCode };

            if (!string.IsNullOrWhiteSpace(bankSignedContractBase64))
                PayResponse = new PayResponseInfo { BankSignedContractBase64 = bankSignedContractBase64 };

            PayResponse = new PayResponseInfo { ReceivedAtUtc = DateTime.UtcNow };
            ;
            Touch(); // به‌روزرسانی UpdatedAtUtc یا هر متد داخلی مشابه
        }


        public void MarkFacilitySubmitted(int? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.FacilitySubmitted, reasonCode, uiMessage);

        public void MarkUnderReview(int? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.UnderReview, reasonCode, uiMessage);

        public void MarkRejected(int? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.Rejected, reasonCode, uiMessage);

        public void SetApprovalCode(decimal approvalCode)
        {
            Contract = new ContractInfo { ApprovalCode = approvalCode };
            Touch();
        }
        public void SetOtpRequirement(bool requiresOtp)
        {
            RequiresOtp = requiresOtp;
            Touch();
        }
        public void SetPayRequestId(string payRequestId)
        {
            if (string.IsNullOrWhiteSpace(payRequestId))
                throw new ArgumentNullException(nameof(payRequestId));


            var current = PayRequest ?? new PayRequestInfo(
                   PayRequestId: null,
                   RequestedAmount: null
               );

     
            PayRequest = current with { PayRequestId = payRequestId };

            Touch();

            Touch();
        }
        public void MarkFacilitySubmitted(int reasonCode, string uiMessage, string payRequestId)
        {
            PayRequest = PayRequest with { PayRequestId = payRequestId };
            TransitionTo(LoanRequestState.FacilitySubmitted, reasonCode, uiMessage);
        }
        public void MarkRemittanceFailed(int? reasonCode, string? uiMessage)
            => TransitionTo(LoanRequestState.Failed, reasonCode, uiMessage);
        // --- Pay Response ---
        public void ApplyPayResponse(
            PayResponseCode code,
            decimal? bankContractNo,
            decimal? approvedAmount,
            string? contractDate,
            decimal? centralBankTraceCode,
            string? bankSignedBase64,
            int reasonCode,
            string? uiMessage)
        {
            PayResponse = new PayResponseInfo
            {
                Code = code,
                ContractDate = contractDate,
                ApprovedLoanAmount = approvedAmount,
                BankContractNo = bankContractNo,
                BankSignedContractBase64 = bankSignedBase64,
                CentralBankTraceCode = centralBankTraceCode,
                ReceivedAtUtc = DateTime.UtcNow
            };

            switch (code)
            {
                case PayResponseCode.Success:
              
                    if (bankContractNo <= 0)
                        Contract = new ContractInfo { CollateralNo = bankContractNo };
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
        public void MarkOtpSent(int reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.OtpSent, reasonCode, uiMessage);

        public void MarkOtpVerified(int reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.OtpVerified, reasonCode, uiMessage);

        // --- Deposit / Remittance ---
        public void RegisterDeposit(int reasonCode, string uiMessage, decimal transactionNumber)
        {
            Transfer = new TransferInfo { TransactionNumber = transactionNumber };
            TransitionTo(LoanRequestState.RemittancePending, reasonCode, uiMessage);
        }

        public void SetRegisterCode(string registerCode)
        {
            Transfer = new TransferInfo { RegisterCode = registerCode };
            Touch();
        }

        public void MarkRemittancePending(int reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.RemittancePending, reasonCode, uiMessage);

        public void MarkRemittanceReturned(int reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.RemittanceReturned, reasonCode, uiMessage);

        public void MarkDisbursed(int reasonCode, string uiMessage)
            => TransitionTo(LoanRequestState.Disbursed, reasonCode, uiMessage);


        public bool CanPerformRepayment(int? otpCode)
        {
            if (RequiresOtp && otpCode == 0)
                return false;
            return true;
        }

        // --- Repayment ---
        public void MarkRepaymentRegistered(int reasonCode, string uiMessage, string? trackNumber, string? accountNo, decimal? amount, DateTime? whenUtc)
        {

            LastRepayment = new RepaymentSnapshot { TrackNumber = trackNumber, AccountNo = accountNo, Amount = amount, WhenUtc = whenUtc ?? DateTime.UtcNow };
            TransitionTo(LoanRequestState.RepaymentRegistered, reasonCode, uiMessage);
        }

        // --- Errors / decision ---
        public void MarkIneligible(int? code, string? message)
        {
            LastDecision = LastDecision with { ErrorMessage = message, ReasonCode = code, ReasonMessage = message };
            TransitionTo(LoanRequestState.Ineligible, code ?? 10005, message);
        }

        public void MarkTemporaryFailure(int? code, string? message)
        {
            LastDecision = LastDecision with { ErrorMessage = message, ReasonCode = code, ReasonMessage = message };
            TransitionTo(LoanRequestState.Failed, code ?? -1, message);
        }

        // --- Generic transition (keeps last decision) ---
        public void TransitionTo(LoanRequestState next, int? reasonCode, string? uiMessage)
        {
            State = next;
            LastReasonCode = reasonCode;
            LastReasonMessage = uiMessage;
            Touch();
        }


        // ===================== Inquiry helpers =====================
        public void SetInquiryRequestId(string requestId)
        {
            InqueryRequest = InqueryRequest with { RequestId = requestId };
            Touch();
        }
        public void SetInquiryDecision(bool allowed, decimal? maxApproved, List<StatusItem> statuses, string expire = null)
        {
            Inquiry = new InquiryInfo { Allowed = allowed, MaxApprovedAmount = maxApproved, Statuses = statuses, ExpireAt = expire };
            Touch();
        }
        // --- Mutators (اختیاری) ---
        public void UpdateCustomer(string? mobile = null, string? postalCode = null, string? birthDate = null, string? gender = null)
        {
            Customer = new CustomerInfo(Customer.NationalCode, birthDate, mobile ?? Customer.Mobile, postalCode ?? Customer.PostalCode, gender ?? Customer.Gender);
            Touch();
        }
    }
    public enum PayResponseCode : int
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
        /// <summary>
        /// مانده حساب با موفقیت از بانک دریافت شد
        /// </summary>
        CreditChecked = 21,

        InProgress = 22,

        /// <summary>
        /// دریافت اقساط از بانک/درگاه در حال انجام است.
        /// این حالت را وقتی بگذارید که سرویس اقساط را صدا زده‌اید
        /// ولی هنوز پاسخ نهایی را ذخیره نکرده‌اید.
        /// </summary>
      
        InstallmentsFetching = 23,   

        /// <summary>
        /// اقساط با موفقیت از بانک دریافت و در سیستم ذخیره شده‌اند.
        /// بعد از این حالت می‌توانید به Active یا وضعیت‌های بازپرداخت بروید.
        /// </summary>
        
        InstallmentsFetched = 24,   

        /// <summary>
        /// بانک پاسخ موقت/قابل‌تکرار داده است و سیستم باید بعداً دوباره
        /// برای دریافت اقساط تلاش کند (retry scheduled).
        /// این حالت را جایگزین Failed کنید وقتی واقعاً قابل‌ریتری است.
        /// </summary>
        InstallmentsRetryPending = 25,
        /// <summary>
        /// صورت حساب با موفقیت دریافت شد
        /// </summary>
        StatementFetching = 26,

        StatementFetched = 27,
NoChange=28
    }
    public enum DepositType : short
    {
        AnyAccount = 1,     // واریز به هر حساب
        FixedAccount = 2,   // واریز به حساب ثابت (مصوبه)
        CustomerAccount = 3 // واریز به حساب مشتری
    }
}

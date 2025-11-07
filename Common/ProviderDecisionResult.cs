

using LoanService.Domain.Entities;

namespace Common;
public record ProviderDecisionResult(
    Result Result,
    LoanRequestState NextState,
    string UiMessage,
    int ReasonCode,
    bool Retryable 
);
//public enum LoanRequestState
//{ /// <summary>
//  /// درخواست تازه ثبت شده است اما هنوز هیچ استعلامی انجام نشده.
//  /// </summary>
//    Requested = 0,

//    /// <summary>
//    /// استعلام مشتری به بانک ارسال شده و در انتظار پاسخ یا مصوبه است.
//    /// </summary>
//    KYCChecked = 1,

//    /// <summary>
//    /// مشتری از نظر قواعد داخلی واجد شرایط اولیه شناخته شده است.
//    /// (Eligibility کامل یا بخشی از آن تایید شده)
//    /// </summary>
//    Eligible = 2,

//    /// <summary>
//    /// مشتری واجد شرایط نیست (مثلاً بدهی یا نمره اعتباری پایین دارد).
//    /// </summary>
//    Ineligible = 3,

//    /// <summary>
//    /// قراردادها (با یا بدون وثیقه) توسط بانک تولید شده و آماده‌ی دانلود یا امضا هستند.
//    /// </summary>
//    ContractsPrepared = 4,

//    /// <summary>
//    /// پیامک OTP برای تأیید کاربر ارسال شده.
//    /// </summary>
//    OtpSent = 5,

//    /// <summary>
//    /// OTP توسط کاربر تأیید شده و حالا می‌توان درخواست تسهیلات را ارسال کرد.
//    /// </summary>
//    OtpVerified = 6,

//    /// <summary>
//    /// درخواست تسهیلات (Facility یا PayRequest) به بانک ارسال شده است.
//    /// </summary>
//    FacilitySubmitted = 7,

//    /// <summary>
//    /// بانک در حال بررسی مصوبه / اعتبارسنجی است (مرحله Pending Approval).
//    /// </summary>
//    UnderReview = 8,

//    /// <summary>
//    /// بانک تسهیلات را تایید کرده است (Approved).
//    /// </summary>
//    Approved = 9,

//    /// <summary>
//    /// بانک درخواست را رد کرده است.
//    /// </summary>
//    Rejected = 10,

//    /// <summary>
//    /// حواله / پرداخت وجه در حال ثبت است.
//    /// </summary>
//    RemittanceRegistering = 11,

//    /// <summary>
//    /// حواله ثبت شده و در انتظار واریز وجه است.
//    /// </summary>
//    RemittancePending = 12,

//    /// <summary>
//    /// وجه با موفقیت واریز شده است.
//    /// </summary>
//    Disbursed = 13,

//    /// <summary>
//    /// قرارداد فعال شده و اقساط در حال بازپرداخت هستند.
//    /// </summary>
//    Active = 14,

//    /// <summary>
//    /// قرارداد بسته شده / وام تسویه شده است.
//    /// </summary>
//    Completed = 15,

//    /// <summary>
//    /// خطای موقت یا اختلال سیستمی (می‌توان Retry کرد).
//    /// </summary>
//    Failed = 16,

//    /// <summary>
//    /// نیاز به بررسی دستی دارد (مثلاً اختلاف اطلاعات هویتی یا مصوبه مبهم).
//    /// </summary>
//    ManualReview = 17

//}

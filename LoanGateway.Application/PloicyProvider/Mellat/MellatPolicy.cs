using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LoanService.Application.PloicyProvider.Mellat;

public class MellatPolicy<TResponse> : IBankPolicy<TResponse> where TResponse : IBankResponse
{
   // private readonly IReadOnlyDictionary<string, BankCodeRule> _rules;
    private static Dictionary<string, Dictionary<string, BankCodeRule>>? _rules;
    public MellatPolicy(IOptions<MellatPolicyOptions> opts, string policyName)
    {


        // لود تنبل
        if (_rules is null)
        {
            var path = opts.Value.RulesPath;
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("MellatPolicy: RulesPath is not configured.");

            // فایل رو بخون
            var json = File.ReadAllText(path);

            // دیسریالایز: ساختار ما این بود:
            // { "MellatPolicyRules": { "CommonRules": { ... }, "CustomerInquiry": { ... } } }
            var root = JsonSerializer.Deserialize<MellatPolicyRoot>(json)
                       ?? throw new InvalidOperationException("Cannot deserialize Mellat policy json.");

            _rules = root.MellatPolicyRules;
        }
    }
    private static readonly HashSet<int> SuccessCodes = new()
{
    12106,  // فایل قرارداد با موفقیت ایجاد شد. (CommonRules)
    15020,  // عملیات با موفقیت اجرا شد. (هرچند NextState رو Failed زدن!)
    10000,  // درخواست استعلام ثبت شد.
    10109,  // استعلام حساب/نظام وظیفه مثبت است.
    10119,  // استعلام سمات مثبت است.
    10178,  // شاهکار: کد ملی با شماره همراه مطابقت دارد.
    10188,  // ثبت احوال مثبت
    10195,  // کدپستی معتبر است.
    11000,  // اعطای تسهیلات با موفقیت انجام شد.
    12200,  // سرویس با موفقیت اجرا شد. (PayResponse / Installments)
    12206,  // قرارداد تسویه شده است.
    13110,  // عملیات واریز وجه با موفقیت اجرا شد.
    21000   // ثبت حواله با موفقیت انجام شد.
};
    private static readonly HashSet<int> RetryableCodes = new()
{
    // CustomerInquiryResult و CommonRules و بقیه
    2,
    100,     // Otp و چند سرویس دیگه
    200,
    10002,
    10003,
    10005,
    10101,
    10102,
    10111,
    10112,
    10121,
    10162,
    10171,
    10181,
    10183,
    10190,   // کدپستی تایید نشد، در صورت اطمینان مجدد تلاش کنید
    10191,
    10192,
    10203,
    10301,
    10303,
    10402,
    11002,
    11004,
    11005,
    11012,
    11013,
    11015,
    12100,
    12101,
    12105,
    12110,
    12201,
    12202,
    12203,
    12205,
    13108,
    15003,
    16005,
    18002
};


    public Result<ProviderDecisionResult> Evaluate(TResponse response)
    {
        if (response is null)
            return Result<ProviderDecisionResult>.Failure(new Error(-1, "پاسخ بانک تهی است."));

        var code = response.MessageCode ?? 0;
        var bankMsg = response.Message ?? "بدون پیام از بانک.";
        var codeKey = code.ToString();

        if (code == 0 && !_rules.ContainsKey(codeKey))
        {
            var ok = new ProviderDecisionResult(
                IsSuccess: true,
                NextState: null,
                UiMessage: bankMsg,
                 ReasonCode: code,
                 Retryable: false

                );
            return Result<ProviderDecisionResult>.Success(ok);
        }
        if (_rules.TryGetValue(codeKey, out var rule))
        {
            var nextState = ParseStateOrNull(rule.NextState);
            bool retryable;
            if (rule.Retryable is not null)
            {
                retryable = rule.Retryable.Value;
            }
            else if (string.Equals(rule.Severity, "Retryable", StringComparison.OrdinalIgnoreCase))
            {
                retryable = true;
            }
            else
            {
                retryable = RetryableCodes.Contains(code);
            }
            bool isSuccess;
            if (rule.IsSuccess==true)
            {
                isSuccess = rule.IsSuccess;
            }
            else
            {
                isSuccess =
                    SuccessCodes.Contains(code) ||
                    (nextState is not null && nextState != LoanRequestState.Failed);
            }

            var decision = new ProviderDecisionResult(
                IsSuccess: isSuccess,
                NextState: nextState,
                UiMessage: rule.UiMessage ?? bankMsg,
                ReasonCode: code,
                Retryable: retryable
            );

            return Result<ProviderDecisionResult>.Success(decision);
        }

        if (SuccessCodes.Contains(code))
        {
            var decision = new ProviderDecisionResult(
                IsSuccess: true,
                NextState: null,
                UiMessage: bankMsg,
                ReasonCode: code,
                Retryable: false
            );
            return Result<ProviderDecisionResult>.Success(decision);
        }
        if (RetryableCodes.Contains(code))
        {
            var decision = new ProviderDecisionResult(
                IsSuccess: false,
                NextState: LoanRequestState.InProgress,
                UiMessage: "اختلال موقت در سرویس بانک. لطفاً دوباره تلاش کنید.",
                ReasonCode: code,
                Retryable: true
            );
            return Result<ProviderDecisionResult>.Success(decision);
        }
        else
        {
            var decision = new ProviderDecisionResult(
                IsSuccess: false,
                NextState: LoanRequestState.Failed,
                UiMessage: "اختلال در سرویس بانک. لطفاً دوباره تلاش کنید.",
                ReasonCode: code,
                Retryable: false
            );
            return Result<ProviderDecisionResult>.Success(decision);
        }
    }

    private static LoanRequestState? ParseStateOrNull(string? stateName)
    {
        if (string.IsNullOrWhiteSpace(stateName))
            return null;

        return Enum.TryParse<LoanRequestState>(stateName, ignoreCase: true, out var st)
            ? st
            : null;
    }
    public class MellatPolicyRoot
    {
        public Dictionary<string, Dictionary<string, BankCodeRule>> MellatPolicyRules { get; set; }
            = new();
    }
}

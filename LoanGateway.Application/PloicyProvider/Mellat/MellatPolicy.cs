using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Entities;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LoanService.Application.PloicyProvider.Mellat;

public class MellatPolicy<TResponse> : IBankPolicy<TResponse> where TResponse : IBankResponse
{
    private readonly Dictionary<string, BankCodeRule> _rulesForThisSection;
    private static Dictionary<string, Dictionary<string, BankCodeRule>>? _rules;
    public MellatPolicy(IOptions<MellatPolicyOptions> opts, string policyName)
    {


        if (_rules is null)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Common", "MellatPolicyRulesData.json");
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            //var json = File.ReadAllText(opts.Value.RulesPath);
            var root = JsonSerializer.Deserialize<MellatPolicyRoot>(json,options)
                       ?? throw new System.Exception("cannot read mellat rules");
            _rules = root.MellatPolicyRules;
        }

        if (!_rules.TryGetValue(policyName, out var section))
            section = new Dictionary<string, BankCodeRule>();

        _rulesForThisSection = section;
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


        if (code == 0 )
        {
            var items = response.GetStatusItems();
            var item = items.FirstOrDefault(x => x.Code != 0) ?? items.FirstOrDefault();
            if (item is not null)
            {
                code = item.Code;
                bankMsg = item.Message ?? bankMsg;
            }
            else
            {
                return Result<ProviderDecisionResult>.Success(new ProviderDecisionResult(
                    IsSuccess: true,
                    NextState: null,
                    UiMessage: bankMsg,
                    ReasonCode: code,
                    Retryable: false
                ));
            }
        }
      
        if (_rulesForThisSection.TryGetValue(codeKey, out var rule))
        {
            var nextState = ParseStateOrNull(rule.NextState);

            bool retryable = rule.Retryable ?? string.Equals(rule.Severity, "Retryable", StringComparison.OrdinalIgnoreCase)
                             || RetryableCodes.Contains(code);

            bool isSuccess = rule.IsSuccess ? rule.IsSuccess : (SuccessCodes.Contains(code) || (nextState is not null && nextState != LoanRequestState.Failed));


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
            return Result<ProviderDecisionResult>.Success(new ProviderDecisionResult(
                IsSuccess: true,
                NextState: null,
                UiMessage: bankMsg,
                ReasonCode: code,
                Retryable: false
            ));
        }

 
        if (RetryableCodes.Contains(code))
        {
            return Result<ProviderDecisionResult>.Success(new ProviderDecisionResult(
                IsSuccess: false,
                NextState: LoanRequestState.InProgress,
                UiMessage: "اختلال موقت در سرویس بانک. لطفاً دوباره تلاش کنید.",
                ReasonCode: code,
                Retryable: true
            ));
        }

    
        return Result<ProviderDecisionResult>.Success(new ProviderDecisionResult(
            IsSuccess: true, 
            NextState: null,
            UiMessage: bankMsg,
            ReasonCode: code,
            Retryable: false
        ));
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

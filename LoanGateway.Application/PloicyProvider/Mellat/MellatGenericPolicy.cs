using Common;
using LoanService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;




namespace LoanService.Application.PloicyProvider.Mellat;

public interface IMellatGenericPolicy<T>
{
    Result<ProviderDecisionResult> Evaluate(T response);
}
//public  class MellatGenericPolicy<T>: IMellatGenericPolicy<T> where T: IBankResponse
//{

//    private readonly IOptions<MellatPolicyOptions> _options;
//    private readonly IConfiguration _config;


//        public MellatGenericPolicy(IConfiguration config, IOptions<MellatPolicyOptions> options )
//        {
//            _config = config;
//        _options = options;

//        }


    //public Result<ProviderDecisionResult> Evaluate(T response)
    //{
    //    if (response is null)
    //        return Result<ProviderDecisionResult>.Failure(new Error("NULL_RESPONSE", "پاسخ بانک تهی است."));

    //    var code = response.MessageCode?.Trim() ?? "UNKNOWN";
    //    var msg = response.Message ?? "بدون پیام از بانک.";

    //    var rulesSection = _config.GetSection($"MellatPolicyRules:{_options.Value.SectionName}");
    //    var rules = rulesSection.Get<Dictionary<string, BankCodeRule>>() ?? new();

    //    if (!rules.TryGetValue(code, out var rule))
    //    {
        
    //        return Result<ProviderDecisionResult>.Failure(
    //            new Error($"MELLAT.UNKNOWN.{code}", $"کد ناشناخته از بانک ملت: {msg}")
    //        );
    //    }

    //    if (!Enum.TryParse<LoanRequestState>(rule.NextState, out var nextState))
    //    {
    //        return Result<ProviderDecisionResult>.Failure(
    //            new Error($"MELLAT.CONFIG.INVALID_STATE.{code}", $"State نامعتبر در پیکربندی: {rule.NextState}")
    //        );
    //    }

    //    var result = new ProviderDecisionResult(
    //        Result.Success(),
    //        nextState,
    //        rule.UiMessage ?? msg,
    //        $"MELLAT.{code}"
    //    );

    //    return Result<ProviderDecisionResult>.Success(result);
    //}
//}



using Common;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Command.GetContractFile;
using LoanService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.PloicyProvider.Mellat
{
    public class MellatPolicy<TResponse> : IBankPolicy<TResponse> where TResponse : IBankResponse
    {
        private readonly IConfiguration _config;
        private readonly string _sectionName;

        public MellatPolicy(IConfiguration config, string sectionName)
        {
            _config = config;
            _sectionName = sectionName;
        }

        public Result<ProviderDecisionResult> Evaluate(TResponse response)
        {


            if (response is null)
                return Result<ProviderDecisionResult>.Failure(new Error(-1, "پاسخ بانک تهی است."));

            var code = response.MessageCode ?? 0;
            var msg = response.Message ?? "بدون پیام از بانک.";

            var sectionRules = _config
        .GetSection($"MellatPolicyRules:{_sectionName}")
        .Get<Dictionary<string, BankCodeRule>>() ?? new();

          
            var commonRules = _config
                .GetSection("MellatPolicyRules:CommonRules")
                .Get<Dictionary<string, BankCodeRule>>() ?? new();

 
            if (commonRules.TryGetValue(code.ToString(), out var commonRule)
                && !sectionRules.ContainsKey(code.ToString()))
            {
                sectionRules[code.ToString()] = commonRule;
            }

            if (!sectionRules.TryGetValue(code.ToString(), out var rule))
                return Result<ProviderDecisionResult>.Failure(new Error(code, $"کد ناشناخته از بانک ملت: {msg}"));

            var nextState = Enum.TryParse(rule.NextState, out LoanRequestState parsed)
                ? parsed
                : LoanRequestState.Unknown;

            return Result<ProviderDecisionResult>.Success(
                new ProviderDecisionResult(Result.Success(), nextState, rule.UiMessage ?? msg, code, rule.Retryable)
            );
        }
    }
}

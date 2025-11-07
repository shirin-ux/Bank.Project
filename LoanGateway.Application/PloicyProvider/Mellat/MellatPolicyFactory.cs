
using LoanService.Application.Contracts;
using LoanService.Application.PloicyProvider.Mellat;
using LoanService.Domain.Enum;
using Microsoft.Extensions.Options;

namespace Common;


public class MellatPolicyFactory : IBankPolicyFactory
{
    private readonly IOptionsMonitor<MellatPolicyRulesOptions> _options;

    private static readonly Dictionary<string, Func<MellatPolicyRulesOptions, OperationRules?>> _accessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["CommonRules"] = o => o.CommonRules,
            ["CustomerInquiry"] = o => o.CustomerInquiry,
            ["CustomerInquiryResult"] = o => o.CustomerInquiryResult,
            ["ContractFileNoCollateral"] = o => o.ContractFileNoCollateral,
            ["ContractFileWithCollateral"] = o => o.ContractFileWithCollateral,
            ["OtpRequest"] = o => o.OtpRequest,
            ["PayRequest"] = o => o.PayRequest,
            ["PayResponse"] = o => o.PayResponse,
            ["Installments"] = o => o.Installments,
            ["DepositRequest"] = o => o.DepositRequest,
            ["RepaymentRequest"] = o => o.RepaymentRequest,
            ["CustomerBilling"] = o => o.CustomerBilling,
            ["PurchaseDetails"] = o => o.PurchaseDetails,
            ["TransferRegister"] = o => o.TransferRegister,
            ["ReturnedTransfers"] = o => o.ReturnedTransfers,
        };

    public MellatPolicyFactory(IOptionsMonitor<MellatPolicyRulesOptions> options)
    {
        _options = options;
    }

    public IBankPolicy<TResponse> CreatePolicy<TResponse>(
        BankProviderType provider,
        string operationName)
        where TResponse : IBankResponse
    {
        if (provider != BankProviderType.Mellat)
            throw new NotSupportedException("Only Mellat is implemented.");

        var cfg = _options.CurrentValue;

        // قواعد مشترک
        var common = cfg.CommonRules ?? new OperationRules();

        // قواعد مخصوص این عملیات
        OperationRules specific = new();
        if (_accessors.TryGetValue(operationName, out var acc))
        {
            specific = acc(cfg) ?? new OperationRules();
        }

        // merge: اول کامن، بعد اختصاصی (اختصاصی override می‌کند)
        var merged = new Dictionary<string, BankCodeRule>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in common)
            merged[kv.Key] = kv.Value;
        foreach (var kv in specific)
            merged[kv.Key] = kv.Value;

        return new MellatPolicy<TResponse>(merged);
    }
}

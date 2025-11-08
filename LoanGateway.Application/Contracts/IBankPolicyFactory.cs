using Common;
using LoanService.Domain.Enum;

namespace LoanService.Application.Contracts;

public interface IBankPolicyFactory
{
    IBankPolicy<T> CreatePolicy<T>(BankProviderType bank, string policyName) where T : IBankResponse;
}

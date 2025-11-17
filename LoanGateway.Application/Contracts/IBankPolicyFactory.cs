using Common;
using LoanService.Domain.Enum.Loan;

namespace LoanService.Application.Contracts;

public interface IBankPolicyFactory
{
    IBankPolicy<T> CreatePolicy<T>(BankProviderType bank, string policyName) where T : IBankResponse;
}

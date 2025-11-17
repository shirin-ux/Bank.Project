using LoanService.Domain.Enum.Loan;

namespace LoanService.Application.Contracts
{

    public interface IBankProviderFactory
    {
        IBankProvider GetProvider(BankProviderType type);
    }
}

using LoanService.Domain.Enum;

namespace LoanService.Application.Contracts
{

    public interface IBankProviderFactory
    {
        IBankProvider GetProvider(BankProviderType type);
    }
}

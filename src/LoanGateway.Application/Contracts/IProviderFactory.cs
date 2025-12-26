using LoanService.Domain.Enum;

namespace LoanService.Application.Contracts
{

    public interface IProviderFactory
    {
        TProvider GetProvider<TProvider>(ProviderType type) where TProvider : class,IProviderBase;
    }
}

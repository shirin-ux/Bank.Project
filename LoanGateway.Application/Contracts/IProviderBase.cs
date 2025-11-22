using LoanService.Domain.Enum;

namespace LoanService.Application.Contracts
{
    public interface IProviderBase
    {
        ProviderType ProviderType { get; }
    }
}

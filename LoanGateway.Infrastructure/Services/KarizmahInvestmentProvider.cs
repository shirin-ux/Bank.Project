using LoanService.Application.Contracts;
using LoanService.Domain.Enum;

namespace LoanService.Infrastructure.Services;

public class KarizmahInvestmentProvider : IProviderBase
{
    public ProviderType ProviderType => ProviderType.Karizmah;


}

using Bank.Mellat.Infrastructure.Services;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using Microsoft.Extensions.DependencyInjection;

namespace LoanService.Infrastructure.Services
{
    public class ProviderFactory : IProviderFactory
    {
        private readonly IServiceProvider _sp;
        public ProviderFactory(IServiceProvider sp) => _sp = sp;


        public TProvider GetProvider<TProvider>(ProviderType type) where TProvider:class,IProviderBase
        {
            IProviderBase service=type switch
            {
                ProviderType.Mellat => _sp.GetRequiredService<MellatBankProvider>(),
                ProviderType.Karizmah => _sp.GetRequiredService<KarizmahInvestmentProvider>(),
                ProviderType.Saman => throw new NotImplementedException(),
                _ => throw new NotSupportedException($"Bank provider '{type}' is not supported.")

            };
            if (service is not TProvider typed)
            {
                throw new InvalidOperationException(
                    $"Provider '{type}' قابلیت '{typeof(TProvider).Name}' را پیاده‌سازی نکرده است.");
            }

            return typed;
        }
    }
}

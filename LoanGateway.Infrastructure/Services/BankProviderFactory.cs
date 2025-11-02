using Bank.Mellat.Infrastructure.Services;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Infrastructure.Services
{
    public class BankProviderFactory : IBankProviderFactory
    {
        private readonly IServiceProvider _sp;
        public BankProviderFactory(IServiceProvider sp) => _sp = sp;


        public IBankProvider GetProvider(BankProviderType type) => type switch
        {
            BankProviderType.Mellat => _sp.GetRequiredService<MellatBankProvider>(),
           // BankProviderType.Saman => _sp.GetRequiredService<SamanBankProvider>(),
            _ => throw new NotSupportedException($"Bank provider '{type}' is not supported.")
        };
    }
}

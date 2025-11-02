using Common;
using LoanService.Application.Contracts;
using LoanService.Application.PloicyProvider.Mellat;
using LoanService.Domain.Enum;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.PloicyProvider;

public class BankPolicyFactory : IBankPolicyFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;

    public BankPolicyFactory(IServiceProvider serviceProvider, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    public IBankPolicy<T> CreatePolicy<T>(BankProviderType bank, string policyName) where T : IBankResponse
    {
        return bank switch
        {
            BankProviderType.Mellat => new MellatPolicy<T>(_config, policyName),
           // BankProviderType.Saman => new SamanPolicy<T>(_config, policyName),
            //_ => new NoBankPolicy<T>()
        };
    }
}

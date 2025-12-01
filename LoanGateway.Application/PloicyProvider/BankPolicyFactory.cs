using Common;
using LoanService.Application.Contracts;
using LoanService.Application.PloicyProvider.Mellat;
using LoanService.Domain.Enum;
using LoanService.Domain.Enum.Loan;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.PloicyProvider;

public class BankPolicyFactory: IBankPolicyFactory 
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<MellatPolicyOptions> _opts;
    public BankPolicyFactory(IServiceProvider serviceProvider, IOptions<MellatPolicyOptions> opts)
    {
        _serviceProvider = serviceProvider;
        //  _config = config;
        _opts = opts;
    }

    public IBankPolicy<TResponse> CreatePolicy<TResponse>(ProviderType bank, string policyName) where TResponse : IBankResponse
    {
        return bank switch
        {
            ProviderType.Mellat => new MellatPolicy<TResponse>(_opts, policyName),
           // BankProviderType.Saman => new SamanPolicy<T>(_config, policyName),

            //_ => new NoBankPolicy<T>()
        };
    }
}

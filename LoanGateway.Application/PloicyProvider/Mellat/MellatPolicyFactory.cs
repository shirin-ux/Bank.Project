
using LoanService.Application.Contracts;
using LoanService.Application.PloicyProvider.Mellat;
using LoanService.Domain.Enum;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Common;


public class MellatPolicyFactory : IBankPolicyFactory
{
    private readonly IOptions<MellatPolicyOptions> _options;

    private readonly ConcurrentDictionary<string, object> _cache = new();
    public MellatPolicyFactory(IOptions<MellatPolicyOptions> options)
    {
        _options = options;
    }

    public IBankPolicy<TResponse> CreatePolicy<TResponse>(BankProviderType provider, string operationName)
     where TResponse : IBankResponse
    {
        if (provider != BankProviderType.Mellat)
            throw new NotSupportedException("Only Mellat is implemented.");

        var key = $"{typeof(TResponse).FullName}_{operationName}";

      
        if (_cache.TryGetValue(key, out var cachedPolicy))
            return (IBankPolicy<TResponse>)cachedPolicy;

     
        var newPolicy = new MellatPolicy<TResponse>(_options, operationName);
        _cache[key] = newPolicy;
        return newPolicy;
    }
}

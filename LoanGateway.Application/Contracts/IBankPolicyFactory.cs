using Common;
using LoanService.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts;

public interface IBankPolicyFactory
{
    IBankPolicy<T> CreatePolicy<T>(BankProviderType bank, string policyName) where T : IBankResponse;
}

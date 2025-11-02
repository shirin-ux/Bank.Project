using LoanService.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    public sealed record ProviderInfo(
        BankProviderType ProviderType,
        decimal? ApprovalCode,
        bool RequiresOtp
    );
}

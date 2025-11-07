using LoanService.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    /// <summary>
    /// فقط اطلاعاتی از بانک ارائه‌دهنده دارد
    /// </summary>
    /// <param name="ProviderType"></param>
    /// <param name="ApprovalCode"></param>
    /// <param name="RequiresOtp"></param>
    public sealed record ProviderInfo(
        BankProviderType ProviderType,
        decimal? ApprovalCode,
        bool RequiresOtp
    );
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    /// <summary>
    /// اطلاعات انتقال وجه (حواله)
    /// </summary>
    /// <param name="RegisterCode"></param>
    /// <param name="TransactionNumber"></param>
    public sealed record TransferInfo(
        string? RegisterCode,
        decimal? TransactionNumber
    );
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    /// <summary>
    /// درخواست پرداخت به بانک
    /// </summary>
    /// <param name="PayRequestId"></param>
    /// <param name="RequestedAmount"></param>
    public sealed record PayRequestInfo(
        string? PayRequestId,
        decimal? RequestedAmount

    );
}

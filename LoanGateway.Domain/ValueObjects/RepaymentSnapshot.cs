using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    //آخرین پرداخت مشتری
    public sealed record RepaymentSnapshot(
        string? TrackNumber,
        string? AccountNo,
        decimal? Amount,
        DateTime? WhenUtc
    );

}

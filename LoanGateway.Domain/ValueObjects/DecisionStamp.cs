using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    public sealed record DecisionStamp( // آخرین Reason/Error برای UI/Debug
        string? ReasonCode,
        string? ReasonMessage,
        string? ErrorCode,
        string? ErrorMessage
    );
}

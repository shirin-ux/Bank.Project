using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    public sealed record DecisionStamp( 
        int? ReasonCode,
        string? ReasonMessage,
        int? ErrorCode,
        string? ErrorMessage
    );
}

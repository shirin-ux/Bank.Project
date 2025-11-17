using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Enum.Investment;

public enum InvestmentOperationStatus
{
    Pending = 0,     // در انتظار (مثلاً افزایش آنلاین قبل از callback)
    Completed = 1,
    Failed = 2
}

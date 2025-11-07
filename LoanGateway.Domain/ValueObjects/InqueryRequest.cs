using LoanService.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    /// <summary>
    /// فقط درخواست استعلام و اطلاعات ساده‌ای دارد
    /// </summary>
    /// <param name="RequestId"></param>
    public sealed record InqueryRequest(string? RequestId);
    
}

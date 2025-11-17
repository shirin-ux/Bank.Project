using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Enum.Investment;

public enum InvestmentState
{
    New = 0,          // تازه ایجاد شده، هنوز شاید سرمایه‌گذاری نشده
    Active = 1,       // دارای سرمایه‌گذاری فعال
    Closed = 2        // حساب بسته شده (اختیاری، برای آینده)
}

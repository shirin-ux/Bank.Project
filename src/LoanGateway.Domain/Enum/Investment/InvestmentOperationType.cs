using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Enum.Investment;

public enum InvestmentOperationType
{
    CreateAccount = 0,
    /// <summary>
    /// افزایش سرمایه با ثبت فیش / پرداخت انجام‌شده
    /// </summary>
    IncreaseDirect = 1,
    /// <summary>
    /// افزایش سرمایه با درگاه آنلاین
    /// </summary>   
    IncreaseOnline = 2,
    /// <summary>
    /// برداشت مستقیم بعد از پرداخت به کاربر
    /// </summary>
    DecreaseDirect = 3     
}

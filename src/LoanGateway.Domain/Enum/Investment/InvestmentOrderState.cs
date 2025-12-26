using System.ComponentModel.DataAnnotations;

namespace LoanService.Domain.Enum.Investment;

public enum InvestmentOrderState : byte
{
    [Display(Name = " ساخته شده، هنوز نرفته درگاه")]
    Created = 1,
    [Display(Name = " لینک درگاه گرفته شده، منتظر پرداخت")]
    PendingPayment = 2,

    [Display(Name = "پرداخت موفق")]
    Paid = 3,

    [Display(Name = "پرداخت ناموفق / خطا")]
    Failed = 4,

    [Display(Name = " لغو شده")]
    Cancelled = 5
}

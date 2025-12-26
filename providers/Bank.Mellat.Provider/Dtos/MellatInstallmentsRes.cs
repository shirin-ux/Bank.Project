using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatInstallmentsRes
    {
        public decimal ContractNumber { get; set; }

        // تعداد کل اقساط
        public short InstallmentsCount { get; set; }

        // تعداد اقساط پرداخت شده
        public short PaidInstallmentCount { get; set; }

        // تعداد اقساط سررسید شده
        public short NotPaidInstallmentCount { get; set; }

        // تاریخ سررسید نزدیک‌ترین قسط
        public string? EarlierInstallmentDate { get; set; }

        // مبلغ قسط بدون جریمه
        public decimal InstallmentAmount { get; set; }

        // بدهی سررسید شده
        public decimal OpenDebtAmount { get; set; }

        // کل مانده بدهی
        public decimal DebtAmount { get; set; }

        // کل مانده بدهی با احتساب تخفیف
        public decimal DiscountedDebtAmount { get; set; }

        // عنوان قرارداد
        public string? ContractDesc { get; set; }

        // فهرست اقساط
        public List<InstallmentInfo>? Installment { get; set; }

        // کد خطای سامانه
        public string? MessageCode { get; set; }

        // پیام خطای سامانه
        public string? Message { get; set; }
    }
    public class InstallmentInfo
    {
        // شماره قسط
        public short InstallmentNo { get; set; }

        // وضعیت پرداخت: 0=پرداخت نشده, 1=پرداخت شده
        public short PaymentState { get; set; }

        // وضعیت سررسید: 0=سررسید نشده, 1=سررسید شده
        public short DueState { get; set; }

        // نوع قسط: 1=عادی, 2=پلکانی, 3=نامساوی, 4=سود مدت اعطا, 5=سود مهلت پرداخت, 6=التزام/هزینه امهالی
        public short Type { get; set; }

        // تاریخ شمسی قسط
        public int DueDate { get; set; }

        // مانده اصل
        public decimal CapitalAmount { get; set; }

        // مانده سود
        public decimal InterestAmount { get; set; }

        // مانده وجه التزام
        public decimal PenaltyAmount { get; set; }

        // کل مانده قسط
        public decimal InstallmentAmount { get; set; }
    }

}

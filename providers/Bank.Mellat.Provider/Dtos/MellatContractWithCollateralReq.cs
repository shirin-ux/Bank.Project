using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatContractWithCollateralReq
    {

        public decimal ApprovalCode { get; set; }

        // کد ملی تسهیلات گیرنده (اجباری) - 10 رقمی
        public string NationalCode { get; set; } = null!;

        // تاریخ تولد شمسی به فرمت YYYY/MM/DD (اجباری)
        public string BirthDate { get; set; } = null!;

        // شماره موبایل 11 رقمی که با "09" شروع می‌شود (اجباری)
        public string MobileNumber { get; set; } = null!;

        // کد پستی 10 رقمی (اجباری)
        public string PostalCode { get; set; } = null!;

        // شماره تماس 11 رقمی که با صفر شروع می‌شود و عدد دوم غیر از 0 و 9 است (اختیاری)
        public string? PhoneNumber { get; set; }

        // مبلغ وام درخواستی (اختیاری، کنترل سقف توسط مصوبه)
        public decimal? LoanAmount { get; set; }

        // تعداد اقساط بازپرداخت مطابق مصوبه (اجباری)
        public short InstallmentCount { get; set; }

        // نوع وثیقه - فقط CHEQUE یا PROMISSORY (اجباری)
        public string CollateralType { get; set; } = null!;

        // شماره یکتای وثیقه 16 رقمی (اجباری)
        public decimal CollateralNo { get; set; }

        // تاریخ وثیقه ثبت شده به شمسی به فرمت YYYY/MM/DD (اجباری)
        public string CollateralDate { get; set; } = null!;

        // مبلغ وثیقه (اجباری)
        public decimal CollateralAmount { get; set; }

        // کد ملی وثیقه گذار 10 رقمی (اجباری)
        public string GuarantorNC { get; set; } = null!;

        // بانک عامل وثیقه (اختیاری)
        public string? CollateralIssuer { get; set; }

        // سری و سریال چک در صورت CHEQUE بودن وثیقه (اختیاری)
        public string? ChequeSerial { get; set; }

        // آدرس (اختیاری، معادل 150 کاراکتر فارسی)
        public string? Address { get; set; }
        public decimal cbTrackingCode { get; set; }

    }
}

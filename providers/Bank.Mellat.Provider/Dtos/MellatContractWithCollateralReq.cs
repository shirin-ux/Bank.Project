using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatContractWithCollateralReq
    {

        public decimal approvalCode { get; set; }

        // کد ملی تسهیلات گیرنده (اجباری) - 10 رقمی
        public string nationalCode { get; set; } = null!;

        // تاریخ تولد شمسی به فرمت YYYY/MM/DD (اجباری)
        public string birthDate { get; set; } = null!;

        // شماره موبایل 11 رقمی که با "09" شروع می‌شود (اجباری)
        public string mobileNumber { get; set; } = null!;
     
        // کد پستی 10 رقمی (اجباری)
        public string postalCode { get; set; } = null!;

        // شماره تماس 11 رقمی که با صفر شروع می‌شود و عدد دوم غیر از 0 و 9 است (اختیاری)
        public string phoneNumber { get; set; }

        // مبلغ وام درخواستی (اختیاری، کنترل سقف توسط مصوبه)
        public decimal? loanAmount { get; set; }

        // تعداد اقساط بازپرداخت مطابق مصوبه (اجباری)
        public short installmentCount { get; set; }

        // نوع وثیقه - فقط CHEQUE یا PROMISSORY (اجباری)
        public string collateralType { get; set; } = null!;

        // شماره یکتای وثیقه 16 رقمی (اجباری)
        public decimal collateralNo { get; set; }

        // تاریخ وثیقه ثبت شده به شمسی به فرمت YYYY/MM/DD (اجباری)
        public string collateralDate { get; set; } = null!;

        // مبلغ وثیقه (اجباری)
        public decimal collateralAmount { get; set; }

        // کد ملی وثیقه گذار 10 رقمی (اجباری)
        public string guarantorNC { get; set; } = null!;

        // بانک عامل وثیقه (اختیاری)
        public string? collateralIssuer { get; set; }

        // سری و سریال چک در صورت CHEQUE بودن وثیقه (اختیاری)
        public string? chequeSerial { get; set; }

        // آدرس (اختیاری، معادل 150 کاراکتر فارسی)
        public string? address { get; set; }
        public decimal? cbTrackingCode { get; set; }

    }
    public static class MellatCollateralTypes
    {
        public const string Cheque = "CHEQUE";
        public const string PromissoryNote = "PROMISSORY_NOTE";
    }
}

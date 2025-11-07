using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatCustomerPurchaseDetailsRes
    {
        public decimal ContractNumber { get; set; } // شماره قرارداد
        public decimal NationalCode { get; set; } // شماره ملی مشتری
        public string CustomerName { get; set; } = string.Empty; // عنوان مشتری
        public int LoanTypeCode { get; set; } // کد نوع تسهیلات
        public string LoanTypeDesc { get; set; } = string.Empty; // شرح نوع تسهیلات
        public decimal ContractAmount { get; set; } // مبلغ قرارداد
        public decimal LoanPaiedAmount { get; set; } // مبلغ تسهیلات اعطایی تاکنون
        public List<ContractDetailDto> ContractDetails { get; set; } = new(); // ریز خریدها
        public string Message { get; set; } = string.Empty; // شرح خطا
        public int MessageCode { get; set; } // کد خطا
    }
    public class ContractDetailDto
    {
        public int TransactionDate { get; set; } // تاریخ خرید/اعطا
        public decimal TransactionNumber { get; set; } // شماره تراکنش خرید/اعطا
        public decimal PayAccNumber { get; set; } // شماره حساب واریز تسهیلات
        public decimal UsedCreditAmount { get; set; } // مبلغ اعتبار مصرفی
        public decimal SellerPaiedAmount { get; set; } // مبلغ واریزی به حساب فروشنده
        public int DocDate { get; set; } // تاریخ سند
        public string SellerNationalCode { get; set; } = string.Empty; // کد ملی فروشنده
        public string SellerName { get; set; } = string.Empty; // عنوان فروشنده
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatCustomerPurchaseDetailsRes
    {
        public decimal contractNumber { get; set; } // شماره قرارداد
        public decimal nationalCode { get; set; } // شماره ملی مشتری
        public string customerName { get; set; } = string.Empty; // عنوان مشتری
        public int loanTypeCode { get; set; } // کد نوع تسهیلات
        public string loanTypeDesc { get; set; } = string.Empty; // شرح نوع تسهیلات
        public decimal contractAmount { get; set; } // مبلغ قرارداد
        public decimal loanPaiedAmount { get; set; } // مبلغ تسهیلات اعطایی تاکنون
        public List<contractDetailDto> ContractDetails { get; set; } = new(); // ریز خریدها
        public string message { get; set; } = string.Empty; // شرح خطا
        public int messageCode { get; set; } // کد خطا
    }
    public class contractDetailDto
    {
        public int transactionDate { get; set; } // تاریخ خرید/اعطا
        public decimal transactionNumber { get; set; } // شماره تراکنش خرید/اعطا
        public decimal payAccNumber { get; set; } // شماره حساب واریز تسهیلات
        public decimal usedCreditAmount { get; set; } // مبلغ اعتبار مصرفی
        public decimal sellerPaiedAmount { get; set; } // مبلغ واریزی به حساب فروشنده
        public int docDate { get; set; } // تاریخ سند
        public string sellerNationalCode { get; set; } = string.Empty; // کد ملی فروشنده
        public string sellerName { get; set; } = string.Empty; // عنوان فروشنده
    }
}

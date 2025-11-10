using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatTransferRegisterReq
    {
        public decimal approvalCode { get; set; } // شماره مصوبه
        public int transferDate { get; set; } // تاریخ حواله یا انتقال وجه (YYYYMMDD)
        public decimal payAmount { get; set; } // مبلغ حواله
        public string destIban { get; set; } = string.Empty; // شماره شبای مقصد
        public string destNationalId { get; set; } = string.Empty; // کد/شناسه ملی مقصد
        public string destName { get; set; } = string.Empty; // نام ذینفع مقصد
        public string description { get; set; } = string.Empty; // توضیحات حواله

        public short? transType { get; init; }
        public List<contractDetails> details { get; set; } = new(); // لیست جزئیات حواله

        public class contractDetails
        {
            public decimal referenceNo { get; set; } // شماره مرجع (شماره قرارداد یا تراکنش)
            public decimal amount { get; set; } // مبلغ تراکنش
        }
    }
}

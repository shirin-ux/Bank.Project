using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatTransferRegisterReq
    {
        public decimal ApprovalCode { get; set; } // شماره مصوبه
        public int TransferDate { get; set; } // تاریخ حواله یا انتقال وجه (YYYYMMDD)
        public decimal PayAmount { get; set; } // مبلغ حواله
        public string DestIban { get; set; } = string.Empty; // شماره شبای مقصد
        public string DestNationalId { get; set; } = string.Empty; // کد/شناسه ملی مقصد
        public string DestName { get; set; } = string.Empty; // نام ذینفع مقصد
        public string Description { get; set; } = string.Empty; // توضیحات حواله

        public short? TransType { get; init; }
        public List<TransferDetailDto> Details { get; set; } = new(); // لیست جزئیات حواله

        public class TransferDetailDto
        {
            public decimal ReferenceNo { get; set; } // شماره مرجع (شماره قرارداد یا تراکنش)
            public decimal Amount { get; set; } // مبلغ تراکنش
        }
    }
}

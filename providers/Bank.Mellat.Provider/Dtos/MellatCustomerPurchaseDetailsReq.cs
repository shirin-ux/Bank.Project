using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatCustomerPurchaseDetailsReq
    {
        public decimal contractNumber { get; set; } // شماره قرارداد
        public string nationalCode { get; set; } = string.Empty; // کد ملی
        public int fromDate { get; set; } // از تاریخ (DDMMYYYY)
        public int toDate { get; set; } // تا تاریخ (DDMMYYYY)
    }
}

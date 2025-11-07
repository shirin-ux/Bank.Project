using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatCustomerPurchaseDetailsReq
    {
        public decimal ContractNumber { get; set; } // شماره قرارداد
        public string NationalCode { get; set; } = string.Empty; // کد ملی
        public int FromDate { get; set; } // از تاریخ (DDMMYYYY)
        public int ToDate { get; set; } // تا تاریخ (DDMMYYYY)
    }
}

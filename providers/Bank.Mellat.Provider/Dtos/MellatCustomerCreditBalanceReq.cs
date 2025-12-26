using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatCustomerCreditBalanceReq
    {
        public decimal contractNumber { get; set; }
        public string nationalCode { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public record MellatCustomerCreditBalanceRes
    {
        public string nationalCode { get; set; } = null!;
        public contractCreditList[] ContractCreditList { get; set; } = Array.Empty<contractCreditList>();

        public int? messageCode { get; set; }
        public string? message { get; set; }

        public record contractCreditList
        {

            public decimal approvalCode { get; set; }

            public decimal contractNumber { get; set; }

            public decimal creditBalance { get; set; }
        }
    }

}

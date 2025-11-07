using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public record MellatCustomerCreditBalanceRes
    {
        public string NationalCode { get; set; } = null!;

        public ContractCreditItem[]? ContractCreditList { get; set; }

        public int? MessageCode { get; set; }
        public string? Message { get; set; }

        public record ContractCreditItem
        {

            public decimal ApprovalCode { get; set; }

            public decimal ContractNumber { get; set; }

            public decimal CreditBalance { get; set; }
        }
    }

}

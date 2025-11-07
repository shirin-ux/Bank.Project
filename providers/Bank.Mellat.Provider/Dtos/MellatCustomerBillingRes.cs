using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public sealed class MellatCustomerBillingRes
    {
        public BillingItem[]? billings { get; set; }
        public string? message { get; set; }
        public int? messageCode { get; set; }

        public sealed class BillingItem
        {
            public string? customerName { get; set; }
            public int? payDeadLine { get; set; }               
            public decimal? totalPurchase { get; set; }
            public decimal? contractNumber { get; set; }
            public decimal? debtPayableInInstallments { get; set; }
            public decimal? payableAmount { get; set; }
            public int? issueDate { get; set; }
            public int? perioadStartDate { get; set; }        
            public int? perioadEndDate { get; set; }
            public short? billingNumber { get; set; }
        }
    }
    
}

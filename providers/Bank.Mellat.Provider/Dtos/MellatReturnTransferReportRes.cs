using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos;

public sealed class MellatReturnTransferReportRes
{
    public ReturnedTransfer[]? returnedTransfers { get; set; }
    public decimal fromId { get; set; }       
    public int? messageCode { get; set; }
    public string? message { get; set; }

    public sealed class ReturnedTransfer
    {
        public string registerCode { get; set; } = default!;
        public decimal approvalId { get; set; }
        public short transferStatus { get; set; }  
        public int? returnReasonCode { get; set; }
        public string? returnReasonDesc { get; set; }
        public string destIban { get; set; } = default!;
        public string destName { get; set; } = default!;
        public short? destBankCode { get; set; }
        public decimal payAmount { get; set; }
        public int? transferDate { get; set; }
        public int? returnDate { get; set; }
        public int? returnTime { get; set; }
        public int? sendDate { get; set; }
        public int? sendTime { get; set; }
        public int? noSendDate { get; set; }
        public short? reasonCode { get; set; }
        public string sourceIban { get; set; } = default!;
        public string? trackingNO { get; set; }
        public int rowId { get; set; }
    }
}
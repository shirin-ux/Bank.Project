using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public sealed class MellatReturnTransferReportReq
    {
        public int returnDate { get; set; }     
        public decimal fromId { get; set; }    
    }
}

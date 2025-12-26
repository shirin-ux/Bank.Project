using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public sealed class MellatSubmitPayRequestReq
    {
        public decimal contractNumber { get; set; }        
        public decimal? requestAmount { get; set; }   
        public string contractFile { get; set; } = default!;
    }
}

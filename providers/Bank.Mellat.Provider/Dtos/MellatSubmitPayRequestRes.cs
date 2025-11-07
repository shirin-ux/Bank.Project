using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public sealed class MellatSubmitPayRequestRes
    {
        public string? payRequestId { get; set; }  
        public string? responseCode { get; set; }   
        public string? responseMessage { get; set; } 
    }
}

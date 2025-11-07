using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatFileUploadRes
    {
        public byte[] ContractFile { get; set; } 
        public decimal ContractNumber { get; set; }
        public string MessageCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

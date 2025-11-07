using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatFileUploadRes
    {
        public byte[] contractFile { get; set; } 
        public decimal contractNumber { get; set; }
        public string messageCode { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
    }
}

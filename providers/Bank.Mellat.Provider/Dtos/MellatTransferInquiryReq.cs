using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatTransferInquiryReq
    {
        public string RegisterCode { get; set; } = string.Empty; // شماره پیگیری (اجباری)
    }
}

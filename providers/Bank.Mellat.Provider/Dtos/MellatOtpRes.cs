using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatOtpRes
    {
        /// <summary>
        /// کد خطای سامانه
        /// </summary>
        public string messageCode { get; set; }

        /// <summary>
        /// پیام خطای سامانه
        /// </summary>
        public string message { get; set; }
    }
}

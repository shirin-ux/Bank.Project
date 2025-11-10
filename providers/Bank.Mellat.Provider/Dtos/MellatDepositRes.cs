using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatDepositRes
    {
        /// <summary>
        /// شماره تراکنش
        /// </summary>
        public decimal transactionNumber { get; set; }

        /// <summary>
        /// کد خطای سامانه
        /// </summary>
        public int messageCode { get; set; }

        /// <summary>
        /// پیام خطای سامانه
        /// </summary>
        public string message { get; set; } = string.Empty;
    }
}

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
        public decimal TransactionNumber { get; set; }

        /// <summary>
        /// کد خطای سامانه
        /// </summary>
        public int MessageCode { get; set; }

        /// <summary>
        /// پیام خطای سامانه
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatOtpReq
    {
        /// <summary>
        /// شماره قرارداد (اجباری)
        /// </summary>
        public decimal contractNumber { get; set; }

        /// <summary>
        /// کد ملی مشتری (اجباری)
        /// </summary>
        public string nationalCode { get; set; } = string.Empty;

        /// <summary>
        /// مبلغ درخواست یا وجه واریزی (اجباری)
        /// </summary>
        public decimal payAmount { get; set; }

        /// <summary>
        /// نوع سرویس (1: واریز وجه، 2: بازپرداخت بدهی)
        /// </summary>
        public short serviceType { get; set; }

        /// <summary>
        /// شماره حساب (اگر ServiceType = 2 باشد، اجباری است)
        /// </summary>
        public string? accountNumber { get; set; }
    }
}

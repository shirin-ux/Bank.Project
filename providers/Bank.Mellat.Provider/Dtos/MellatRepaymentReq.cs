using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatRepaymentReq
    {
        /// <summary>
        /// شماره حساب (اجباری)
        /// </summary>
        public decimal accountNo { get; set; }

 

        /// <summary>
        /// شماره قرارداد (اجباری)
        /// </summary>
        public decimal contractNo { get; set; }

        /// <summary>
        /// کد ملی مشتری (اجباری)
        /// </summary>
        public string nationalCode { get; set; } = string.Empty;

        /// <summary>
        /// مبلغ بازپرداخت (اجباری)
        /// </summary>
        public decimal repaymentAmount { get; set; }

        /// <summary>
        /// کد یکبار مصرف (اختیاری - ممکن است طبق تنظیمات مصوبه اجباری شود)
        /// </summary>
        public int? otpCode { get; set; }
    }
}

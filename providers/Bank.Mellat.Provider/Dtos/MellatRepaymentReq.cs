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
        public decimal AccountNo { get; set; }

 

        /// <summary>
        /// شماره قرارداد (اجباری)
        /// </summary>
        public decimal ContractNo { get; set; }

        /// <summary>
        /// کد ملی مشتری (اجباری)
        /// </summary>
        public string NationalCode { get; set; } = string.Empty;

        /// <summary>
        /// مبلغ بازپرداخت (اجباری)
        /// </summary>
        public decimal RepaymentAmount { get; set; }

        /// <summary>
        /// کد یکبار مصرف (اختیاری - ممکن است طبق تنظیمات مصوبه اجباری شود)
        /// </summary>
        public int? OtpCode { get; set; }
    }
}

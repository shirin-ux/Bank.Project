using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
  public  class MellatDepositReq
    {
        /// <summary>
        /// شماره قرارداد (اجباری)
        /// </summary>
        public decimal ContractNumber { get; set; }

        /// <summary>
        /// کد ملی مشتری (اجباری)
        /// </summary>
        public string BuyerNationalCode { get; set; } = string.Empty;

        /// <summary>
        /// کد ملی فروشنده (اختیاری - اگر DepositType = 1 باشد اجباری است)
        /// </summary>
        public string? SellerNationalCode { get; set; }

        /// <summary>
        /// شماره حساب فروشنده (اختیاری - اگر DepositType = 1 باشد اجباری است)
        /// </summary>
        public decimal? SellerAccountNo { get; set; }

        /// <summary>
        /// مبلغ وجه واریزی (اجباری)
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// شرح تراکنش (اختیاری)
        /// </summary>
        public string? TransactionDesc { get; set; }

        /// <summary>
        /// نوع واریز:
        /// 1: واریز به هر حساب
        /// 2: واریز به حساب ثابت
        /// 3: واریز به حساب مشتری
        /// (در صورتی که در مصوبه بیش از یک روش پرداخت مشخص شده باشد، اجباری است)
        /// </summary>
        public short? DepositType { get; set; }

        /// <summary>
        /// کد یکبار مصرف (اختیاری - بسته به تنظیمات مصوبه ممکن است اجباری باشد)
        /// </summary>
        public int? OtpCode { get; set; }
    }
}

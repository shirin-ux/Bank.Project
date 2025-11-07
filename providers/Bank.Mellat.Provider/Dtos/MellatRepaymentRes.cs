namespace Bank.Mellat.Provider.Dtos
{
    public class MellatRepaymentRes
    {
        /// <summary>
        /// زمان بازپرداخت (Timestamp)
        /// </summary>
        public DateTime RepaymentDate { get; set; }

        /// <summary>
        /// شماره حساب
        /// </summary>
        public decimal AccountNumber { get; set; }

        /// <summary>
        /// شماره پیگیری سامانه
        /// </summary>
        public int TrackNumber { get; set; }

        /// <summary>
        /// شماره قرارداد
        /// </summary>
        public decimal ContractNumber { get; set; }

        /// <summary>
        /// مبلغ بازپرداختی
        /// </summary>
        public decimal RepaymentAmount { get; set; }

        /// <summary>
        /// نام صاحب حساب
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// شرح خطا (در صورت وجود)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// کد خطا
        /// </summary>
        public int MessageCode { get; set; }
    }
}

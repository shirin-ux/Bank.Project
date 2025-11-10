namespace Bank.Mellat.Provider.Dtos
{
    public class MellatRepaymentRes
    {
        /// <summary>
        /// زمان بازپرداخت (Timestamp)
        /// </summary>
        public DateTime repaymentDate { get; set; }

        /// <summary>
        /// شماره حساب
        /// </summary>
        public decimal accountNumber { get; set; }

        /// <summary>
        /// شماره پیگیری سامانه
        /// </summary>
        public int trackNumber { get; set; }

        /// <summary>
        /// شماره قرارداد
        /// </summary>
        public decimal contractNumber { get; set; }

        /// <summary>
        /// مبلغ بازپرداختی
        /// </summary>
        public decimal repaymentAmount { get; set; }

        /// <summary>
        /// نام صاحب حساب
        /// </summary>
        public string customerName { get; set; } = string.Empty;

        /// <summary>
        /// شرح خطا (در صورت وجود)
        /// </summary>
        public string message { get; set; } = string.Empty;

        /// <summary>
        /// کد خطا
        /// </summary>
        public int messageCode { get; set; }
    }
}

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatTransferInquiryRes
    {
        public List<transferInquiryDetailDto> inquiryDetails { get; set; } = new(); // جزئیات وضعیت حواله
        public string registerCode { get; set; } = string.Empty; // شماره پیگیری
        public decimal aprovalCode { get; set; } // شماره مصوبه
        public int messageCode { get; set; } // کد خطا یا پیام سامانه
        public string message { get; set; } = string.Empty; // توضیحات یا پیام سامانه

        public class transferInquiryDetailDto
        {
            public short transferStatus { get; set; } // وضعیت حواله

            public decimal payAmount { get; set; } // مبلغ حواله
            public string destIban { get; set; } = string.Empty; // شماره شبا مقصد
            public string destName { get; set; } = string.Empty; // نام صاحب حساب مقصد
            public string destNationalId { get; set; } = string.Empty; // کد ملی ذینفع مقصد
            public int transType { get; set; } // نوع حواله (۱: پایا، ۲: ساتنا، ...)
            public string description { get; set; } = string.Empty; // شرح مقصد
            public int sendDate { get; set; } // تاریخ ارسال (YYYYMMDD)
            public int sendTime { get; set; } // زمان ارسال (HHMMSS)
            public int? returnDate { get; set; } // تاریخ بازگشت (اختیاری)
            public int? returnTime { get; set; } // زمان بازگشت (اختیاری)
            public int? deleteDate { get; set; } // تاریخ حذف (اختیاری)
            public int? deleteTime { get; set; } // زمان حذف (اختیاری)
            public string trackingNo { get; set; } = string.Empty; // شماره پیگیری بین‌بانکی
            public int? returnReasonCode { get; set; } // علت برگشت (در صورت وجود)
        }
    }

}

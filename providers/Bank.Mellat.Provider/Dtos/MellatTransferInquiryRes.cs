namespace Bank.Mellat.Provider.Dtos
{
    public class MellatTransferInquiryRes
    {
        public List<TransferInquiryDetailDto> InquiryDetails { get; set; } = new(); // جزئیات وضعیت حواله
        public string RegisterCode { get; set; } = string.Empty; // شماره پیگیری
        public decimal ApprovalCode { get; set; } // شماره مصوبه
        public int MessageCode { get; set; } // کد خطا یا پیام سامانه
        public string Message { get; set; } = string.Empty; // توضیحات یا پیام سامانه

        public class TransferInquiryDetailDto
        {
            public short TransferStatus { get; set; } // وضعیت حواله

            public decimal PayAmount { get; set; } // مبلغ حواله
            public string DestIban { get; set; } = string.Empty; // شماره شبا مقصد
            public string DestName { get; set; } = string.Empty; // نام صاحب حساب مقصد
            public string DestNationalId { get; set; } = string.Empty; // کد ملی ذینفع مقصد
            public int TransType { get; set; } // نوع حواله (۱: پایا، ۲: ساتنا، ...)
            public string Description { get; set; } = string.Empty; // شرح مقصد
            public int SendDate { get; set; } // تاریخ ارسال (YYYYMMDD)
            public int SendTime { get; set; } // زمان ارسال (HHMMSS)
            public int? ReturnDate { get; set; } // تاریخ بازگشت (اختیاری)
            public int? ReturnTime { get; set; } // زمان بازگشت (اختیاری)
            public int? DeleteDate { get; set; } // تاریخ حذف (اختیاری)
            public int? DeleteTime { get; set; } // زمان حذف (اختیاری)
            public string TrackingNo { get; set; } = string.Empty; // شماره پیگیری بین‌بانکی
            public int? ReturnReasonCode { get; set; } // علت برگشت (در صورت وجود)
        }
    }

}

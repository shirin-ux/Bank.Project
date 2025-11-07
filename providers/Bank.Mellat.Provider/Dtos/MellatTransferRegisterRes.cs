using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
   public class MellatTransferRegisterRes
    {
        public string RegisterCode { get; set; } = string.Empty; // شماره پیگیری
        public short TransType { get; set; } // نوع حواله (1: پایا، 2: ساتنا، 3: پل، 4: ملت به ملت)
        public List<string> ContractsError { get; set; } = new(); // لیست قراردادهای نامعتبر
        public List<string> TransactionsError { get; set; } = new(); // لیست تراکنش‌های نامعتبر
        public int MessageCode { get; set; } // کد خطا یا پاسخ
        public string Message { get; set; } = string.Empty; // توضیحات خطا یا پیام پاسخ
    }
}

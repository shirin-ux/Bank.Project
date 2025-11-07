using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatCustomerBillingReq
    {
        /// <summary>
        /// شماره صورتحساب
        /// اگر 0 باشد، 30 صورتحساب آخر قرارداد بازگردانده می‌شود.
        /// اگر عددی غیر از 0 باشد، 30 صورتحساب کوچکتر از آن عدد برگردانده می‌شود.
        /// </summary>
        public short BillingNumber { get; set; }

        /// <summary>
        /// شماره قرارداد (اجباری)
        /// </summary>
        public decimal ContractNumber { get; set; }

        /// <summary>
        /// کد ملی مشتری (اجباری)
        /// </summary>
        public string NationalCode { get; set; } = string.Empty;
    }
}

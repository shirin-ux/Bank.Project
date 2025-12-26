using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karizmah.Provider.Dtos
{
   public class KarizmahOrderRequestDto
    {
        public Guid? id { get; set; }

        public string? planTypeAliasName { get; set; }

        public string? coverageAliasName { get; set; }

        public Guid? planTypeId { get; set; }

        public Guid? coverageId { get; set; }

        public long? wealthPolicyId { get; set; }

        public long? lifePolicyId { get; set; }

        public string? nationalCode { get; set; }

        public string? phoneNumber { get; set; }

        /// <summary>
        /// اگر در مستند کاریزما به‌صورت GUID است، می‌توانی نوع را Guid? بگذاری
        /// فعلاً طبق متن Rest، string در نظر گرفتیم.
        /// </summary>
        public string? customerBankAccountId { get; set; }

        /// <summary>
        /// بازه سنی از / تا
        /// </summary>
        public int? ageFrom { get; set; }
        public int? ageTo { get; set; }

        /// <summary>
        /// بازه مبلغی از / تا (ریال)
        /// </summary>
        public decimal? amountFrom { get; set; }
        public decimal? amountTo { get; set; }

        public int? coefficient { get; set; }

        public Guid? referenceId { get; set; }

        /// <summary>
        /// وضعیت سفارش، مثلاً: "Unknown", "Done", "Pending"
        /// بهتر است در لایه Application برای آن Enum بسازی و اینجا متنش را بفرستی.
        /// </summary>
        public string? status { get; set; }

        /// <summary>
        /// نوع سفارش، مثلاً: "Decrease", "Increase", "Buy", "Swap"
        /// </summary>
        public string? orderType { get; set; }

        public string? description { get; set; }

        public DateTimeOffset? createDateStartDate { get; set; }
        public DateTimeOffset? createDateEndDate { get; set; }

        public DateTimeOffset? modifyDateStartDate { get; set; }
        public DateTimeOffset? modifyDateEndDate { get; set; }

        public string? paymentUrl { get; set; }

        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
}

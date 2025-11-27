namespace Karizmah.Provider.Dtos;

public class KarizmahOrderResponseDto : BaseResponse<KarizmahOrderResponseDto>
{
    public List<KarizmahOrderItemDto> karizmahOrderItems { get; set; }
    public int page { get; set; }
    public int pageSize { get; set; }
    public int totalCount { get; set; }
    public sealed class KarizmahOrderItemDto
    {
        public Guid id { get; set; }

        public string planType { get; set; } = default!;

        public string coverage { get; set; } = default!;

        public string nationalCode { get; set; } = default!;

        public string planTypeAliasName { get; set; } = default!;

        public string coverageAliasName { get; set; } = default!;

        public int age { get; set; }

        public long backofficeOrderId { get; set; }

        public long wealthPolicyId { get; set; }

        public long lifePolicyId { get; set; }

        public Guid planTypeId { get; set; }

        public Guid coverageId { get; set; }

        public double amount { get; set; }   // اگر خواستی برای پول decimal بذار

        public int coefficient { get; set; }

        public string referenceId { get; set; } = default!;

        public string clientId { get; set; } = default!;

        public string phoneNumber { get; set; } = default!;

        public int status { get; set; }

        public string statusTitle { get; set; } = default!;

        public string description { get; set; } = default!;

        public string paymentUrl { get; set; } = default!;

        public int orderType { get; set; }

        public string createDate { get; set; } = default!;   // در صورت نیاز می‌تونی به DateTime تبدیلش کنی

        public string modifyDate { get; set; } = default!;

        public string birthDate { get; set; } = default!;

        public string callbackUrl { get; set; } = default!;

        public string receiptDate { get; set; } = default!;

        public string receiptNumber { get; set; } = default!;

        public long traceId { get; set; }
    }
}

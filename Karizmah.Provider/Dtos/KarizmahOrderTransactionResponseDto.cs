namespace Karizmah.Provider.Dtos
{
    public class KarizmahOrderTransactionResponseDto : BaseResponse<KarizmahOrderTransactionResponseDto>
    {
        public List<KarizmahOrderItemDto> karizmahOrderItems { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int totalCount { get; set; }
        public sealed class KarizmahOrderItemDto
        {
            public long traceId { get; set; }

            public Guid id { get; set; }

            public string planType { get; set; } = default!;

            public string coverage { get; set; } = default!;

            public string nationalCode { get; set; } = default!;

            public string planTypeAliasName { get; set; } = default!;

            public string coverageAliasName { get; set; } = default!;

            public long wealthPolicyId { get; set; }

            public long lifePolicyId { get; set; }

            public Guid planTypeId { get; set; }

            public Guid coverageId { get; set; }

            public double amount { get; set; }

            public string referenceId { get; set; } = default!;

            public int status { get; set; }

            public string statusTitle { get; set; } = default!;

            public int orderType { get; set; }
            public string createDate { get; set; } = default!;
            public string modifyDate { get; set; } = default!;
        }
    }
}

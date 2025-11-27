namespace Karizmah.Provider.Dtos
{
    public class KarizmahOrderTransactionRequestDto
    {
        public Guid? id { get; set; }

        public string? nationalCode { get; set; }

        public string? planTypeAliasName { get; set; }

        public string? coverageAliasName { get; set; }

        public long? backofficeOrderId { get; set; }

        public long? wealthPolicyId { get; set; }

        public long? lifePolicyId { get; set; }

        public Guid? planTypeId { get; set; }

        public Guid? coverageId { get; set; }

        public string? referenceId { get; set; }

        public long? traceId { get; set; }

        public string? status { get; set; }      // مثلاً "Done"
        public string? orderType { get; set; }   // مثلاً "Decrease"

        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
}

namespace LoanGateway.Auth.Domain.Entities
{
    public class VerfiyMobileOwnerInquiry : BaseEntity
    {
        public string NationalId { get; set; } = default!;
        public string MobileNumber { get; set; } = default!;

        public bool? IsMatched { get; set; }

        public int? StatusCode { get; set; }
        public string? StatusMessage { get; set; }

        public string? RequestId { get; set; }
        public string? CorrelationId { get; set; }

        public string? RawResponseJson { get; set; }
    }
}

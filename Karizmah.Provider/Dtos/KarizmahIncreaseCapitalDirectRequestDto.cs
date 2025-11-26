

namespace Karizmah.Provider.Dtos
{
    public sealed class KarizmahIncreaseCapitalDirectRequestDto
    {
        public decimal amount { get; set; }
        public long policyId { get; set; }
        public DateTime rceiptDate { get; set; } = default!;
        public string receiptNumber { get; set; } = default!;
        public long traceId { get; set; }
        public string? description { get; set; }
    }

}

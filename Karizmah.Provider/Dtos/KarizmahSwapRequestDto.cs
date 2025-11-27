namespace Karizmah.Provider.Dtos;

public sealed class KarizmahSwapRequestDto
{
    public decimal amount { get; set; }
    public long traceId { get; set; }
    public string nationalCode { get; set; } = default!;
    public string? description { get; set; }
    public long sourcePolicyId { get; set; }

    public long destinationPolicyId { get; set; }
}


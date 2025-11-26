namespace Karizmah.Provider.Dtos;

public sealed class KarizmahIncreaseRequestDto
{
    public decimal amount { get; set; }
    public long policyId { get; set; }
    public long traceId { get; set; }
    public string? description { get; set; }
    public string callbackUrl { get; set; }  ///
}

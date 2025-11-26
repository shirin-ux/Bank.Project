namespace Karizmah.Provider.Dtos;

public sealed class KarizmahOrderBuyRequestDto
{
    public int age { get; set; } = 0;
    public long amount { get; set; }
    public int coefficient { get; set; } = 0;
    public string coverageAliasName { get; set; } = "0";
    public string? description { get; set; }
    public long traceId { get; set; }
    public string nationalCode { get; set; } = default!;
    public string planTypeAliasName { get; set; } = default!;
    public string birthDate { get; set; } = default!;
    public string callbackUrl { get; set; } = default!;
    public string? address { get; set; }
    public string? phoneNumber { get; set; }
    public string? utm { get; set; }
}

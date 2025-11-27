namespace Karizmah.Provider.Dtos;

public sealed class KarizmahPolicyHistoryRequestDto
{
    public long policyId { get; set; }
    public DateTime fromDate { get; set; }
    public DateTime toDate { get; set; }
}

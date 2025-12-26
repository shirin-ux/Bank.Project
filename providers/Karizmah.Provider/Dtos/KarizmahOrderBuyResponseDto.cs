namespace Karizmah.Provider.Dtos;

public class KarizmahOrderBuyResponseDto : BaseResponse<KarizmahOrderBuyResponseDto>
{

    public string birthDate { get; set; }
    public string nationalCode { get; set; }
    public string description { get; set; }
    public long amount { get; set; }

    public long traceId { get; set; }

}

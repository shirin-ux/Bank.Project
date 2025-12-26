namespace Karizmah.Provider.Dtos;

public class KarizmahOrderRevokableAmountResponseDto:BaseResponse<KarizmahOrderRevokableAmountResponseDto>
{
    public decimal value { get; set; }
    public decimal revokable { get; set; }
}

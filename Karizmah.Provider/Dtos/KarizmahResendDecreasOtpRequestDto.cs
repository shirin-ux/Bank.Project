namespace Karizmah.Provider.Dtos;

public sealed class KarizmahResendDecreasOtpRequestDto
{
    public string orderId { get; set; } // از سرویس برداشت غیر مستقیم میاد
    public string nationalCode { get; set; }
}

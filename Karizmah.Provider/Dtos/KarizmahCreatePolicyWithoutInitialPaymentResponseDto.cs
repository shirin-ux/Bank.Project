namespace Karizmah.Provider.Dtos;

public sealed class KarizmahCreatePolicyWithoutInitialPaymentResponseDto:BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>
{
    public string policyId { get; set; } = default!;
    public long traceId { get; set; }
    public bool isRepeated { get; set; }

}

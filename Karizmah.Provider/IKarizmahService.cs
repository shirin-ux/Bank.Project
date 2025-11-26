using Karizmah.Provider.Dtos;

namespace Karizmah.Provider;

public interface IKarizmahService
{
    Task<BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>> CreatePolicyWithoutInitialPaymentAsync( KarizmahCreatePolicyWithoutInitialPaymentRequestDto req,CancellationToken ct);
    Task<BaseResponse<KarizmahOrderBuyResponseDto>> BuyOrderAsync(KarizmahOrderBuyRequestDto req, CancellationToken ct);
    
    Task<KarizmaTokenResponse> GetAccessTokenAsync(CancellationToken ct);
    Task<BaseResponse<KarizmahTraceIdResponse>> GenerateTraceIdAsync(CancellationToken ct);
    Task<BaseResponse<KarizmahIncreaseCapitalDirectResponseDto>> IncreaseCapitalDirectAsync(KarizmahIncreaseCapitalDirectRequestDto req,CancellationToken ct);
    Task<BaseResponse<KarizmahIncreaseResponseDto>> IncreaseAsync(KarizmahIncreaseRequestDto req,CancellationToken ct);
    Task<BaseResponse<KarizmahDecreaseDirectResponseDto>> DecreaseDirectAsync(KarizmahDecreaseDirectRequestDto req,CancellationToken ct);
    Task<BaseResponse<KarizmahDecreaseVerifyResponseDto>> DecreaseVerifyAsync(KarizmahDecreaseVerifyRequestDto req,CancellationToken ct);

}


using SadadProvider.Dto;

namespace SadadProvider;

public interface ISadadService
{
    Task<SadadPaymentRequestResultDto> PaymentRequestAsync(SadadPaymentRequestDto dto, CancellationToken ct);
    Task<SadadVerifyResultDto> VerifyAsync(string token, CancellationToken ct);
}

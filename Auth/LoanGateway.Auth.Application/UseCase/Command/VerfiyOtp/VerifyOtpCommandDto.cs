using Common;
using LoanGateway.Auth.Domain.Enum;
using MediatR;

namespace LoanGateway.Auth.Application.UseCase.Command.VerfiyOtp
{
    public sealed class VerifyOtpCommandDto : IRequest<Result<VerifyOtpResultDto>>
    {
        public string MobileNumber { get; init; } = default!;
        public string Code { get; init; } = default!;
        public OtpPurpose Purpose { get; init; }
    }
}

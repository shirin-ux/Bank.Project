using LoanGateway.Auth.Domain.Enum;

namespace LoanGateway.Auth.Application.UseCase.Command.VerfiyOtp
{
    public sealed class VerifyOtpResultDto
    {
        public bool IsValid { get; init; }
        public bool IsBlocked { get; init; }
        public Guid? UserId { get; init; }

        public OtpPurpose Purpose { get; init; }
        public bool IsProfileCompleted { get; init; }
        public bool IsNewUser { get; init; }

        public string AccessToken { get; init; } = default!;
        public DateTime AccessTokenExpiresAtUtc { get; init; }

        public string RefreshToken { get; init; } = default!;
        public DateTime RefreshTokenExpiresAtUtc { get; init; }
    }
}

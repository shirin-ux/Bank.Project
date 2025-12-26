namespace LoanGateway.Auth.Domain.Entities;

public sealed class RefreshTokens : BaseEntity
{
    public Guid UserId { get; set; }
    public byte[] TokenHash { get; set; } = default!;
    public Guid JwtId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public DateTime? RotatedAtUtc { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime AccessTokenExpiresAtUtc { get; set; }

}


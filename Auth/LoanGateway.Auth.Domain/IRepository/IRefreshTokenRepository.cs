using LoanGateway.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.IRepository
{
    public interface IRefreshTokenRepository
    {
        Task InsertAsync(RefreshTokens token, CancellationToken ct);

        Task<RefreshTokens?> GetActiveByHashAsync(byte[] tokenHash, CancellationToken ct);

        Task RevokeAsync(Guid id, DateTime revokedAtUtc, string? reason, Guid? replacedByTokenId, CancellationToken ct);

        Task RevokeByHashAsync(byte[]? tokenHash, DateTime revokedAtUtc, string? reason, CancellationToken ct);


        Task<RefreshTokens?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(RefreshTokens entity, CancellationToken ct = default);
        Task<RefreshTokens?> GetByHashIncludingRevokedAsync(byte[] tokenHash, CancellationToken ct);

        Task<RefreshTokens> GetTokenPairAsync(Guid refreshTokenId, CancellationToken ct);
        Task<RefreshTokens?> GetByPlainRefreshTokenAsync(string refreshToken, CancellationToken ct);
    }
       
}

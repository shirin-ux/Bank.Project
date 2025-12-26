using Microsoft.EntityFrameworkCore;
using System.Data;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using LoanGateway.Auth.Domain;

namespace LoanGateway.Auth.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenRepository(AuthDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshTokens?> GetActiveByHashAsync(byte[] tokenHash, CancellationToken ct)
    {
        return await _context.RefreshTokens
           
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<RefreshTokens?> GetByHashIncludingRevokedAsync(byte[] tokenHash, CancellationToken ct)
    {
        return await _context.RefreshTokens

            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<RefreshTokens?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task InsertAsync(RefreshTokens token, CancellationToken ct)
    {
        if (token.Id == Guid.Empty)
            token.Id = Guid.NewGuid();

        _context.RefreshTokens.Add(token);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RevokeAsync(Guid id, DateTime revokedAtUtc, string? reason, Guid? replacedByTokenId, CancellationToken ct)
    {
        var token = await _context.RefreshTokens.FindAsync(new object[] { id }, ct);
        if (token != null && token.RevokedAtUtc == null)
        {
            token.RevokedAtUtc = revokedAtUtc;
            token.RevokedReason = reason;
            token.ReplacedByTokenId = replacedByTokenId;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeByHashAsync(byte[]? tokenHash, DateTime revokedAtUtc, string? reason, CancellationToken ct)
    {
        var token = await _context.RefreshTokens
            .Where(x => x.RevokedAtUtc == null)
            .FirstOrDefaultAsync(ct);

        if (token != null)
        {
            token.RevokedAtUtc = revokedAtUtc;
            token.RevokedReason = reason;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateAsync(RefreshTokens entity, CancellationToken ct = default)
    {
        var existing = await _context.RefreshTokens.FindAsync(new object[] { entity.Id }, ct);
        if (existing != null)
        {
            existing.RefreshToken = entity.RefreshToken;
            existing.AccessToken = entity.AccessToken;
            existing.JwtId = entity.JwtId;
            existing.ExpiresAtUtc = entity.ExpiresAtUtc;
            //existing.CreatedAtUtc = entity.CreatedAtUtc;
            existing.RevokedAtUtc = entity.RevokedAtUtc;
            existing.RotatedAtUtc = entity.RotatedAtUtc;
            existing.RevokedReason = entity.RevokedReason;
            existing.ReplacedByTokenId = entity.ReplacedByTokenId;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task<RefreshTokens> GetTokenPairAsync(Guid refreshTokenId, CancellationToken ct)
    {
        return await _context.RefreshTokens
            .Where(x => x.Id == refreshTokenId)
            .Select(x => new RefreshTokens
            {
                Id = x.Id,
                AccessToken = x.AccessToken,
                RefreshToken = x.RefreshToken,
                AccessTokenExpiresAtUtc = x.AccessTokenExpiresAtUtc,
                ExpiresAtUtc = x.ExpiresAtUtc
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<RefreshTokens?> GetByPlainRefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        refreshToken = refreshToken.Trim();

        return await _context.RefreshTokens
            .Where(x => x.RefreshToken == refreshToken)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
    }
}

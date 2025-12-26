using Dapper;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using System.Data;

namespace LoanGateway.Auth.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility;

        public RefreshTokenRepository(TransactionDBUtility transactionDBUtility)
        {
            _transactionDBUtility = transactionDBUtility;
        }


        public async Task<RefreshTokens?> GetActiveByHashAsync(
         byte[] tokenHash,CancellationToken ct)
        {
            const string sql = """
        SELECT TOP (1) *
        FROM [dbo].[RefreshTokens]
        WHERE TokenHash = @TokenHash
        ORDER BY CreatedAtUtc DESC;
        """;


            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryFirstOrDefaultAsync<RefreshTokens>(
                new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: ct));
        }
        public async Task<RefreshTokens?> GetByHashIncludingRevokedAsync(byte[] tokenHash, CancellationToken ct)
        {
            const string sql = """
        SELECT TOP (1) *
        FROM [dbo].[RefreshTokens]
        WHERE TokenHash = @TokenHash
        ORDER BY CreatedAtUtc DESC;
        """;

            var parameters = new DynamicParameters();
            parameters.Add("@TokenHash", tokenHash, DbType.Binary, size: tokenHash.Length);

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryFirstOrDefaultAsync<RefreshTokens>(
                new CommandDefinition(sql, parameters, cancellationToken: ct));
        }
        public async Task<RefreshTokens?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {

            const string sql = @"SELECT *
                         FROM [dbo].[RefreshTokens]
                         WHERE Id = @Id;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            var result = await conn.QueryFirstOrDefaultAsync<RefreshTokens>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

            return result;
        }

        public async Task InsertAsync(RefreshTokens token, CancellationToken ct)
        {
            const string sql = @"INSERT INTO [dbo].[RefreshTokens]
                                (Id, UserId,AccessToken,RefreshToken,JwtId, ExpiresAtUtc,CreatedAtUtc,RevokedAtUtc, RevokedReason)
                                VALUES
                                (@Id, @UserId, @AccessToken,@RefreshToken, @JwtId, @ExpiresAtUtc,  @CreatedAtUtc, @RevokedAtUtc, @RevokedReason);";

            if (token.Id == Guid.Empty)
                token.Id = Guid.NewGuid();
            await using var conn = _transactionDBUtility.GetSqlConnection();

            await conn.OpenAsync(ct);

            await conn.ExecuteAsync(
                new CommandDefinition(sql, token, cancellationToken: ct));
        }

        public async Task RevokeAsync(Guid id, DateTime revokedAtUtc, string? reason, Guid? replacedByTokenId, CancellationToken ct)
        {
            const string sql = @"UPDATE [dbo].[RefreshTokens]
                             SET RevokedAtUtc = @RevokedAtUtc,
                                 RevokedReason = @RevokedReason,
                                 ReplacedByTokenId = @ReplacedByTokenId
                             WHERE Id = @Id AND RevokedAtUtc IS NULL;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new { Id = id, RevokedAtUtc = revokedAtUtc, RevokedReason = reason, ReplacedByTokenId = replacedByTokenId }, cancellationToken: ct));

        }

        public async Task RevokeByHashAsync(byte[] tokenHash, DateTime revokedAtUtc, string? reason, CancellationToken ct)
        {
            const string sql = @"UPDATE [dbo].[RefreshTokens]
                             SET RevokedAtUtc = @RevokedAtUtc,
                                 RevokedReason = @RevokedReason
                             WHERE TokenHash = @TokenHash AND RevokedAtUtc IS NULL;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new { TokenHash = tokenHash, RevokedAtUtc = revokedAtUtc, RevokedReason = reason }, cancellationToken: ct));

        }

        public async Task UpdateAsync(RefreshTokens entity, CancellationToken ct = default)
        {
            const string sql = @"UPDATE [dbo].[RefreshTokens]
                             SET RefreshToken = @RefreshToken,
                                 AccessToken = @AccessToken,
                                 JwtId = @JwtId,
                                 ExpiresAtUtc = @ExpiresAtUtc,
                                 CreatedAtUtc = @CreatedAtUtc,
                                 RevokedAtUtc = @RevokedAtUtc,
                                 RotatedAtUtc = @RotatedAtUtc,
                                 RevokedReason = @RevokedReason,
                                 ReplacedByTokenId = @ReplacedByTokenId
                             WHERE Id = @Id;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new
                {
                    entity.Id,
                    entity.AccessToken,
                    entity.RefreshToken,
                    entity.JwtId,
                    entity.ExpiresAtUtc,
                    entity.CreatedAtUtc,
                    entity.RevokedAtUtc,
                    entity.RotatedAtUtc,
                    entity.RevokedReason,
                    entity.ReplacedByTokenId
                }, cancellationToken: ct));
        }

        public async Task<RefreshTokens> GetTokenPairAsync(Guid refreshTokenId, CancellationToken ct)
        {
            const string sql = @"
        SELECT AccessToken, RefreshToken, AccessTokenExpiresAtUtc, ExpiresAtUtc
        FROM [dbo].[RefreshTokens]
        WHERE Id = @Id;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryFirstOrDefaultAsync(sql, new { Id = refreshTokenId });

        }

        public async Task<RefreshTokens?> GetByPlainRefreshTokenAsync(string refreshToken, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            // Trim کردن whitespace ها برای اطمینان از تطابق دقیق
            refreshToken = refreshToken.Trim();

            const string sql = @"
        SELECT TOP 1 *
        FROM [dbo].[RefreshTokens]
        WHERE RefreshToken = @RefreshToken
        ORDER BY CreatedAtUtc DESC;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            var result = await conn.QueryFirstOrDefaultAsync<RefreshTokens>(
                new CommandDefinition(sql, new { RefreshToken = refreshToken }, cancellationToken: ct));

            return result;
        }
    }
}

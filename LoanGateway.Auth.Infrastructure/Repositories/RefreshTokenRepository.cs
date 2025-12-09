using Dapper;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;

namespace LoanGateway.Auth.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility;

        public RefreshTokenRepository(TransactionDBUtility transactionDBUtility)
        {
            _transactionDBUtility = transactionDBUtility;
        }
        public async Task InsertAsync(RefreshToken token, CancellationToken ct)
        {
            const string sql = @"INSERT INTO [dbo].[RefreshToken]
                                (Id, UserId,TokenHash,JwtId, ExpiresAtUtc,CreatedAtUtc,RevokedAtUtc, RevokedReason)
                                VALUES
                                (@Id, @UserId, @TokenHash, @JwtId, @ExpiresAtUtc,  @CreatedAtUtc, @RevokedAtUtc, @RevokedReason);";

            if (token.Id == Guid.Empty)
                token.Id = Guid.NewGuid();
            await using var conn = _transactionDBUtility.GetSqlConnection();

            await conn.OpenAsync(ct);

            await conn.ExecuteAsync(
                new CommandDefinition(sql, token, cancellationToken: ct));
        }
    }
}


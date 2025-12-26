using Dapper;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;

namespace LoanGateway.Auth.Infrastructure.Repositories;

public sealed class GiftCardEligibleUserRepository : IGiftCardEligibleUserRepository
{
    private readonly TransactionDBUtility _transactionDBUtility;

    public GiftCardEligibleUserRepository(TransactionDBUtility transactionDBUtility)
    {
        _transactionDBUtility = transactionDBUtility;
    }

    public async Task<bool> IsEligibleByNationalCodeAsync(string nationalCode, CancellationToken ct)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM [dbo].[GiftCardEligibleUsers]
                WHERE NationalCode = @NationalCode
            ) THEN 1 ELSE 0 END;";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);

        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { NationalCode = nationalCode }, cancellationToken: ct));
    }

    public async Task MarkAsProcessedAsync(string nationalCode, CancellationToken ct)
    {
        const string sql = @"
            UPDATE [dbo].[GiftCardEligibleUsers]
            SET 
                IsProcessed = 1,
                ProcessedAtUtc = SYSUTCDATETIME()
            WHERE NationalCode = @NationalCode;";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { NationalCode = nationalCode }, cancellationToken: ct));
    }
}










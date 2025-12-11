using Dapper;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Infrastructure.Repositories;
// Auth.Infrastructure/Repositories/UserProfileRepository.cs
public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly TransactionDBUtility _transactionDBUtility;

    public UserProfileRepository(TransactionDBUtility transactionDBUtility)
    {
        _transactionDBUtility = transactionDBUtility;

    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"SELECT TOP (1) *FROM [User] WHERE Id = @Id;";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);

        return await conn.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<bool> ExistsByNationalCodeAsync(
        string nationalCode,
        Guid? excludeUserId,
        CancellationToken ct)
    {
        const string sql = @"
                            SELECT CASE WHEN EXISTS(
                                SELECT 1 FROM [User]
                                WHERE NationalCode = @NationalCode
                                  AND (@ExcludeUserId IS NULL OR Id <> @ExcludeUserId)
                            ) THEN 1 ELSE 0 END;";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);

        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new
            {
                NationalCode = nationalCode,
                ExcludeUserId = excludeUserId
            }, cancellationToken: ct));
    }

    public async Task UpdateProfileAfterKycAsync(User user, CancellationToken ct)
    {
        const string sql = @"
                           UPDATE [User]
                           SET
                               NationalCode       = @NationalCode,
                               FirstName          = @FirstName,
                               LastName           = @LastName,
                               BirthDate          = @BirthDate,
                               IsProfileCompleted = @IsProfileCompleted,
                               IsKycVerified      = @IsKycVerified,
                               UpdatedAtUtc       = @UpdatedAtUtc
                           WHERE Id = @Id;";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                user.Id,
                user.NationalCode,
                user.FirstName,
                user.LastName,
                user.BirthDate,
                user.IsProfileCompleted,
                user.IsKycVerified,
                user.UpdatedAtUtc
            }, cancellationToken: ct));
    }
}

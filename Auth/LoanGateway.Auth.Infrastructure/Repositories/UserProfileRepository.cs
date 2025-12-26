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

    public async Task<User?> GetByNationalCodeAsync(string nationalCode, CancellationToken ct)
    {
        const string sql = @"SELECT TOP (1) * FROM [User] WHERE NationalCode = @nationalCode;";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);

        return await conn.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { NationalCode = nationalCode }, cancellationToken: ct));
    }

    public async Task<bool> ExistsByNationalCodeAsync(
        string nationalCode,
        string mobileNumber,
        Guid? excludeUserId,
        CancellationToken ct)
    {
        const string sql = @"
                            SELECT CASE WHEN EXISTS(
                                SELECT 1 FROM [User]
                                WHERE NationalCode = @NationalCode and MobileNumber <> @MobileNumber
                                  AND (@ExcludeUserId IS NULL OR Id <> @ExcludeUserId)
                            ) THEN 1 ELSE 0 END;";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);

        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new
            {
                NationalCode = nationalCode,
                ExcludeUserId = excludeUserId,
                MobileNumber=mobileNumber,
            }, cancellationToken: ct));
    }

    public async Task UpdateProfileAfterKycAsync(User user, CancellationToken ct)
    {
        // استفاده از try-catch برای مدیریت GiftStatus (ممکن است ستون وجود نداشته باشد)
        try
        {
            const string sql = @"
                               UPDATE [User]
                               SET
                                   ShenasnamehNumber=@ShenasnamehNumber,
                                   ShenasnameSeri=@ShenasnameSeri,
                                   ShenasnameSerial=@ShenasnameSerial,
                                   PostalCode         = @PostalCode,
                                   NationalCode       = @NationalCode,
                                   FirstName          = @FirstName,
                                   LastName           = @LastName,
                                   BirthDate          = @BirthDate,
                                   IsProfileCompleted = @IsProfileCompleted,
                                   IsKycVerified      = @IsKycVerified,
                                   GiftStatus         = @GiftStatus,
                                   UpdatedAtUtc       = @UpdatedAtUtc
                               WHERE Id = @Id;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new
                {
                    user.Id,
                    user.NationalCode,
                    user.PostalCode,
                    user.FirstName,
                    user.LastName,
                    user.BirthDate,
                    user.IsProfileCompleted,
                    user.IsKycVerified,
                    user.ShenasnameSerial,
                    user.ShenasnameSeri,
                    user.ShenasnamehNumber,
                    GiftStatus = user.GiftStatus ?? false,
                    user.UpdatedAtUtc
                }, cancellationToken: ct));
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
        {
            // اگر ستون GiftStatus وجود نداشت، بدون آن update می‌کنیم
            const string sqlWithoutGiftStatus = @"
                               UPDATE [User]
                               SET
                                   ShenasnamehNumber=@ShenasnamehNumber,
                                   ShenasnameSeri=@ShenasnameSeri,
                                   ShenasnameSerial=@ShenasnameSerial,
                                   PostalCode         = @PostalCode,
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
                new CommandDefinition(sqlWithoutGiftStatus, new
                {
                    user.Id,
                    user.NationalCode,
                    user.PostalCode,
                    user.FirstName,
                    user.LastName,
                    user.BirthDate,
                    user.IsProfileCompleted,
                    user.IsKycVerified,
                    user.ShenasnameSerial,
                    user.ShenasnameSeri,
                    user.ShenasnamehNumber,
                    user.UpdatedAtUtc
                }, cancellationToken: ct));
        }
    }
}

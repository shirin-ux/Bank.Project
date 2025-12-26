using Dapper;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Numerics;

namespace LoanGateway.Auth.Infrastructure.Repositories
{
    public sealed class UserReadRepository : IUserReadRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility;

        public UserReadRepository(TransactionDBUtility transactionDBUtility)
        {
            _transactionDBUtility = transactionDBUtility;
        }

        public async Task<bool> ExistsByMobileOrNationalCodeAsync(string mobileNumber, string nationalCode, CancellationToken ct)
        {
            const string sql = @"SELECT TOP (1) 1 FROM [User] WHERE IsDeleted = 0 AND (MobileNumber = @MobileNumber OR NationalCode = @NationalCode);";

            await using var conn = _transactionDBUtility.GetSqlConnection();

            var result = await conn.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { MobileNumber = mobileNumber, NationalCode = nationalCode }, cancellationToken: ct));

            return result.HasValue;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            try
            {
                const string sqlWithGiftStatus = @"
                                               SELECT TOP(1)
                                                   Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                                   IsMobileVerified, IsProfileCompleted, KycLevel, IsActive,
                                                   ShenasnameSerial,ShenasnameSeri,ShenasnamehNumber,
                                                   LastLoginAtUtc, IsKycVerified, BirthDate,
                                                   CreatedAtUtc, UpdatedAtUtc,
                                                   ISNULL(CAST(GiftStatus AS bit), 0) AS GiftStatus
                                               FROM [dbo].[User] 
                                               WHERE Id = @Id
                                               AND (IsDeleted = 0 OR IsDeleted IS NULL);";

                return await conn.QueryFirstOrDefaultAsync<User>(
                    new CommandDefinition(sqlWithGiftStatus, new { Id = id }, cancellationToken: ct));
            }
            catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
            {
                const string sqlWithoutGiftStatus = @"
                                                   SELECT TOP(1)
                                                       Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                                       IsMobileVerified, IsProfileCompleted, KycLevel, IsActive,
                                                       ShenasnameSerial,ShenasnameSeri,ShenasnamehNumber,
                                                       LastLoginAtUtc, IsKycVerified, BirthDate,
                                                       CreatedAtUtc, UpdatedAtUtc,
                                                       0 AS GiftStatus
                                                   FROM [dbo].[User] 
                                                   WHERE Id = @Id
                                                       AND (IsDeleted = 0 OR IsDeleted IS NULL);";

                return await conn.QueryFirstOrDefaultAsync<User>(
                    new CommandDefinition(sqlWithoutGiftStatus, new { Id = id }, cancellationToken: ct));
            }
        }
        

        public async Task<User?> GetByMobileAsync(string mobileNumber, CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);


            try
            {
                const string sqlWithGiftStatus = @"
                                                SELECT TOP(1) 
                                                    Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                                    IsMobileVerified, IsProfileCompleted, KycLevel, IsActive,
                                                    ShenasnameSerial,ShenasnameSeri,ShenasnamehNumber,
                                                    LastLoginAtUtc, IsKycVerified, BirthDate,
                                                    CreatedAtUtc, UpdatedAtUtc,
                                                    ISNULL(CAST(GiftStatus AS bit), 0) AS GiftStatus
                                                FROM [dbo].[User] 
                                                WHERE MobileNumber = @MobileNumber
                                                AND (IsDeleted = 0 OR IsDeleted IS NULL);";

                return await conn.QueryFirstOrDefaultAsync<User>(
                    new CommandDefinition(sqlWithGiftStatus, new { MobileNumber = mobileNumber }, cancellationToken: ct));
            }
            catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
            {
                const string sqlWithoutGiftStatus = @"
                                                    SELECT TOP(1) 
                                                        Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                                        IsMobileVerified, IsProfileCompleted, KycLevel, IsActive,
                                                        ShenasnameSerial,ShenasnameSeri,ShenasnamehNumber,
                                                        LastLoginAtUtc, IsKycVerified, BirthDate,
                                                        CreatedAtUtc, UpdatedAtUtc,
                                                        0 AS GiftStatus
                                                    FROM [dbo].[User] 
                                                    WHERE MobileNumber = @MobileNumber
                                                        AND (IsDeleted = 0 OR IsDeleted IS NULL);";

                return await conn.QueryFirstOrDefaultAsync<User>(
                    new CommandDefinition(sqlWithoutGiftStatus, new { MobileNumber = mobileNumber }, cancellationToken: ct));
            }
        }

        public async Task<User?> GetByNationalCodeAsync(string nationalCode, CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            try
            {
                const string sqlWithGiftStatus = @"
                                                 SELECT TOP(1)
                                                     Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                                     IsMobileVerified, IsProfileCompleted, KycLevel, IsActive,
                                                     LastLoginAtUtc, IsKycVerified, BirthDate,
                                                     ShenasnameSerial,ShenasnameSeri,ShenasnamehNumber,
                                                     CreatedAtUtc, UpdatedAtUtc,
                                                     ISNULL(CAST(GiftStatus AS bit), 0) AS GiftStatus
                                                 FROM [dbo].[User] 
                                                 WHERE NationalCode = @NationalCode
                                                     AND (IsDeleted = 0 OR IsDeleted IS NULL);";

                return await conn.QueryFirstOrDefaultAsync<User>(
                    new CommandDefinition(sqlWithGiftStatus, new { NationalCode = nationalCode }, cancellationToken: ct));
            }
            catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
            {
                const string sqlWithoutGiftStatus = @"
                                                    SELECT TOP(1)
                                                        Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                                        IsMobileVerified, IsProfileCompleted, KycLevel, IsActive,
                                                        LastLoginAtUtc, IsKycVerified, BirthDate,
                                                        ShenasnameSerial,ShenasnameSeri,ShenasnamehNumber,
                                                        CreatedAtUtc, UpdatedAtUtc,
                                                        0 AS GiftStatus
                                                    FROM [dbo].[User] 
                                                    WHERE NationalCode = @NationalCode
                                                        AND (IsDeleted = 0 OR IsDeleted IS NULL);";

                return await conn.QueryFirstOrDefaultAsync<User>(
                    new CommandDefinition(sqlWithoutGiftStatus, new { NationalCode = nationalCode }, cancellationToken: ct));
            }
        }



        public async Task<User?> GetUserAsync(CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            try
            {
                const string sqlWithGiftStatus = @"
                                                 SELECT TOP(1)
                                                     Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                                     IsMobileVerified, IsProfileCompleted, KycLevel, IsActive,
                                                     LastLoginAtUtc, IsKycVerified, BirthDate,
                                                     ShenasnameSerial,ShenasnameSeri,ShenasnamehNumber,
                                                     CreatedAtUtc, UpdatedAtUtc,
                                                     ISNULL(CAST(GiftStatus AS bit), 0) AS GiftStatus
                                                 FROM [dbo].[User] 
                                                     WHERE (IsDeleted = 0 OR IsDeleted IS NULL);";

                return await conn.QueryFirstOrDefaultAsync<User>(
                    new CommandDefinition(sqlWithGiftStatus, cancellationToken: ct));
            }
            catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
            {
                const string sqlWithoutGiftStatus = @"
                                                    SELECT TOP(1)
                                                        Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                                        IsMobileVerified, IsProfileCompleted, KycLevel, IsActive,
                                                        LastLoginAtUtc, IsKycVerified, BirthDate,
                                                        ShenasnameSerial,ShenasnameSeri,ShenasnamehNumber,
                                                        CreatedAtUtc, UpdatedAtUtc,
                                                        0 AS GiftStatus
                                                    FROM [dbo].[User] 
                                                    WHERE 
                                                         (IsDeleted = 0 OR IsDeleted IS NULL);";

                return await conn.QueryFirstOrDefaultAsync<User>(
                    new CommandDefinition(sqlWithoutGiftStatus, cancellationToken: ct));
            }
        }




        public async Task<User> InsertAsync(User user, CancellationToken ct)
        {
            try
            {

                const string sql = @"
                                   INSERT INTO [dbo].[User]
                                    (Id, MobileNumber, NationalCode, PostalCode, FirstName, LastName,
                                       IsMobileVerified, IsActive, CreatedAtUtc, UpdatedAtUtc)
                                  VALUES
                                   (@Id, @MobileNumber, @NationalCode, @PostalCode, @FirstName, @LastName,
                                         @IsMobileVerified, @IsActive, @CreatedAtUtc, @UpdatedAtUtc);";

                if (user.Id == Guid.Empty)
                    user.Id = Guid.NewGuid();
                await using var conn = _transactionDBUtility.GetSqlConnection();
                await conn.OpenAsync(ct);

                await conn.ExecuteAsync(
                    new CommandDefinition(sql, user, cancellationToken: ct));

                return user;
            }
            catch (SqlException ex) when (ex.Number == 2627) 
            {
                throw; 
            }
            catch (SqlException ex)
            {
                throw new Exception($"خطا در ذخیره کاربر جدید در دیتابیس. SqlError {ex.Number}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("خطای داخلی در هنگام ایجاد کاربر جدید.", ex);
            }
        }

        public async Task UpdateProfileAsync(User user, CancellationToken ct)
        {
            try
            {
                const string sql = @"UPDATE [dbo].[User]
                                                 SET 
                                                   PostalCode=@PostalCode
                                                 WHERE Id = @Id;";


                await using var conn = _transactionDBUtility.GetSqlConnection();
                await conn.OpenAsync(ct);

                await conn.ExecuteAsync(
                    new CommandDefinition(sql, user, cancellationToken: ct));
            }
            catch (Exception EX)
            {

                throw;
            }
   
        }

    }

}

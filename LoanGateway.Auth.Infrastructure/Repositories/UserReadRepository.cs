using Dapper;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
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
            const string sql = @"SELECT * FROM [dbo].[User] WHERE Id = @Id;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryFirstOrDefaultAsync<User>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        }
        

        public async Task<User?> GetByMobileAsync(string mobileNumber, CancellationToken ct)
        {
            const string sql = @"SELECT TOP(1) *FROM [dbo].[User] WHERE MobileNumber = @MobileNumber;";
            await using var conn = _transactionDBUtility.GetSqlConnection();

            await conn.OpenAsync(ct);

            return await conn.QueryFirstOrDefaultAsync<User>(
                new CommandDefinition(sql, new { MobileNumber = mobileNumber }, cancellationToken: ct));
        }

        public async Task<User> InsertAsync(User user, CancellationToken ct)
        {
            try
            {
                const string sql = @"INSERT INTO [dbo].[User]
                               (Id,MobileNumber,NationalCode, FirstName, LastName,IsMobileVerified, IsActive,CreatedAtUtc,UpdatedAtUtc)
                               VALUES
                               (@Id, @MobileNumber,@NationalCode,@FirstName,@LastName, @IsMobileVerified, @IsActive,@CreatedAtUtc,@UpdatedAtUtc
                               );";

                if (user.Id == Guid.Empty)
                    user.Id = Guid.NewGuid();
                await using var conn = _transactionDBUtility.GetSqlConnection();
                await conn.OpenAsync(ct);

                await conn.ExecuteAsync(
                    new CommandDefinition(sql, user, cancellationToken: ct));

                return user;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateProfileAsync(User user, CancellationToken ct)
        {
            const string sql = @"UPDATE [dbo].[User]
                                                 SET 
                                                     NationalCode      = @NationalCode,
                                                     FirstName         = @FirstName,
                                                     LastName          = @LastName,
                                                     BirthDate         = @BirthDate,
                                                     IsProfileCompleted = @IsProfileCompleted,
                                                     UpdatedAtUtc      = @UpdatedAtUtc
                                                 WHERE Id = @Id;";


            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            await conn.ExecuteAsync(
                new CommandDefinition(sql, user, cancellationToken: ct));
        }

    }

}

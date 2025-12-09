
using Dapper;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.Enum;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
namespace LoanGateway.Auth.Infrastructure.Repositories;

public sealed class UserOtpRepository : IUserOtpRepository
{
    private readonly TransactionDBUtility _transactionDBUtility;

    public UserOtpRepository(TransactionDBUtility transactionDBUtility)
    {
        _transactionDBUtility = transactionDBUtility;
    }

    public async Task<int> CountRequestsInWindowAsync(string phoneNumber,OtpPurpose purpose,DateTime utcFrom, CancellationToken ct = default)
    {
        try
        {
            const string sql = @"SELECT COUNT(*) FROM [dbo].[OtpCode] WHERE PhoneNumber = @PhoneNumber  AND Purpose = @Purpose AND CreatedAtUtc >= @FromUtc  AND IsDeleted = 0;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            return await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        PhoneNumber = phoneNumber,
                        Purpose = (byte)purpose,
                        FromUtc = utcFrom
                    },
                    cancellationToken: ct));
        }
        catch (Exception ex)
        {

            throw;
        }
  
    }

    public async Task<OtpCode> GetActiveAsync(string phoneNumber,OtpPurpose purpose,DateTime nowUtc,CancellationToken ct = default)
    {
        try
        {
            const string sql = @"SELECT TOP(1) * FROM [dbo].[OtpCode] WHERE PhoneNumber = @phoneNumber AND Purpose = @purpose AND IsDeleted = 0 AND ConsumedAtUtc IS NULL
                                                                                                                    AND ExpiresAtUtc > @nowUtc ORDER BY CreatedAtUtc DESC;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            var test= await conn.QueryFirstOrDefaultAsync<OtpCode>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        PhoneNumber = phoneNumber,
                        Purpose = (byte)purpose,
                        NowUtc = nowUtc
                    },
                    cancellationToken: ct));
            return test;
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message) ;
        }

 
    }

    public async Task InsertAsync(OtpCode otp, CancellationToken ct = default)
    {
        const string sql = @"
                       INSERT INTO [dbo].[OtpCode]
                       (Id,UserId, PhoneNumber, Purpose, CodeHash,ExpiresAtUtc, CreatedAtUtc, ConsumedAtUtc,FailedAttempts,MaxAttempts, RequestIp, UserAgent,IsDeleted
                       )
                       VALUES
                       (@Id, @UserId, @PhoneNumber, @Purpose, @CodeHash, @ExpiresAtUtc, @CreatedAtUtc,@ConsumedAtUtc,@FailedAttempts, @MaxAttempts, @RequestIp,@UserAgent,@IsDeleted);";

  
        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            otp.Id,
            otp.UserId,
            otp.PhoneNumber,
            Purpose = (byte)otp.Purpose,
            otp.CodeHash,
            otp.ExpiresAtUtc,
            otp.CreatedAtUtc,
            otp.ConsumedAtUtc,
            otp.FailedAttempts,
            otp.MaxAttempts,
            otp.RequestIp,
            otp.UserAgent,
            otp.IsDeleted
        }, cancellationToken: ct));
    }

    public  async Task MarkConsumedAsync(Guid id, DateTime consumedAtUtc, CancellationToken ct)
    {
        const string sql = @"UPDATE dbo.OtpCode SET ConsumedAtUtc = @ConsumedAtUtc WHERE Id = @Id;";
        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);

        await conn.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Id = id, ConsumedAtUtc = consumedAtUtc },
                cancellationToken: ct));
    }
    

    public async Task UpdateAsync(OtpCode otp, CancellationToken ct = default)
    {

        const string sql = @"UPDATE [dbo].[OtpCode]
                              SET
                                  UserId         = @UserId,
                                  ConsumedAtUtc  = @ConsumedAtUtc,
                                  FailedAttempts = @FailedAttempts,
                                  MaxAttempts    = @MaxAttempts,
                                  RequestIp      = @RequestIp,
                                  UserAgent      = @UserAgent,
                                  IsDeleted      = @IsDeleted
                              WHERE Id = @Id;";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    otp.Id,
                    otp.UserId,
                    otp.ConsumedAtUtc,
                    otp.FailedAttempts,
                    otp.MaxAttempts,
                    otp.RequestIp,
                    otp.UserAgent,
                    otp.IsDeleted
                },
                cancellationToken: ct));
    }

    public async Task UpdateFailedAttemptsAsync(Guid id, int failedAttempts, CancellationToken ct)
    {
        const string sql = @"UPDATE dbo.OtpCode SET FailedAttempts = @FailedAttempts WHERE Id = @Id;";
        await using var conn = _transactionDBUtility.GetSqlConnection();

        await conn.OpenAsync(ct);

        await conn.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Id = id, FailedAttempts = failedAttempts },
                cancellationToken: ct));
    }

    public bool Verify(string code, string phoneNumber, int purpose, byte[] storedHash)
    {
        throw new NotImplementedException();
    }
}



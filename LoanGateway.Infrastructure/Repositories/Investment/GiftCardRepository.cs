using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.IRepository.Investment;
using static System.Net.WebRequestMethods;

namespace LoanService.Infrastructure.Repositories.Investment;

public sealed class GiftCardRepository(TransactionDBUtility transactionDBUtility) : IGiftCardRepository
{
    private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;


    public async Task<GiftCard?> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        const string sql = @"
            SELECT TOP 1 
                Id, UserId, ReceivedAtUtc, Description, IsReceived, OrderId,IsPending,
                CreatedAtUtc, UpdatedAtUtc, RowVersion
            FROM [dbo].[GiftCard]
            WHERE UserId = @UserId
            ORDER BY CreatedAtUtc DESC;";

        await using var conn = _transactionDBUtility.GetSqlConnection1(); // KhanoumiCore
        await conn.OpenAsync(ct);

        try
        {
            return await conn.QueryFirstOrDefaultAsync<GiftCard>(
                new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            // اگر جدول GiftCard وجود نداشت، null برمی‌گرداند
            return null;
        }
    }

    public async Task<bool> HasUserReceivedGiftAsync(Guid userId, CancellationToken ct)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM [dbo].[GiftCard]
            WHERE UserId = @UserId 
                AND IsReceived = 1;";

        await using var conn = _transactionDBUtility.GetSqlConnection1(); // KhanoumiCore
        await conn.OpenAsync(ct);

        try
        {
            var count = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

            return count > 0;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            // اگر جدول GiftCard وجود نداشت، یعنی کاربر هنوز کارت دریافت نکرده است
            return false;
        }
    }

    public async Task<GiftCard> CreateAsync(GiftCard giftCard, CancellationToken ct)
    {
        // اگر Id set نشده باشد، یک GUID جدید ایجاد می‌کنیم
        if (giftCard.Id == Guid.Empty)
        {
            giftCard.Id = Guid.NewGuid();
        }

        const string sql = @"
            INSERT INTO [dbo].[GiftCard] 
                (Id, UserId, ReceivedAtUtc, Description, IsReceived, IsPending, OrderId, CreatedAtUtc, UpdatedAtUtc)
            VALUES 
                (@Id, @UserId, @ReceivedAtUtc, @Description, @IsReceived, @IsPending, @OrderId, @CreatedAtUtc, @UpdatedAtUtc);";
        
        await using var conn = _transactionDBUtility.GetSqlConnection1(); // KhanoumiCore
        await conn.OpenAsync(ct);

        try
        {
            await conn.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = giftCard.Id,
                UserId = giftCard.UserId,
                UpdatedAtUtc = giftCard.UpdatedAtUtc,
                CreatedAtUtc = giftCard.CreatedAtUtc,
                ReceivedAtUtc = giftCard.ReceivedAtUtc,
                IsReceived = giftCard.IsReceived,
                Description = giftCard.Description,
                OrderId = giftCard.OrderId,
                IsPending = giftCard.IsPending
            }, cancellationToken: ct));

            return giftCard;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            throw new InvalidOperationException(
                "جدول GiftCard در دیتابیس وجود ندارد. لطفاً اسکریپت CreateGiftCardTable.sql را اجرا کنید.", ex);
        }
    }

    public async Task<bool> TryCreateIfNotExistsAsync(GiftCard giftCard, CancellationToken ct)
    {
        // استفاده از MERGE برای عملیات Atomic (جلوگیری از Race Condition)
        const string sql = @"
            MERGE [dbo].[GiftCard] AS target
            USING (SELECT @UserId AS UserId) AS source
            ON target.UserId = source.UserId
            WHEN NOT MATCHED THEN
                INSERT (UserId, ReceivedAtUtc, Description, IsReceived, IsPending, OrderId, CreatedAtUtc, UpdatedAtUtc)
                VALUES (@UserId, @ReceivedAtUtc, @Description, @IsReceived, @IsPending, @OrderId, @CreatedAtUtc, @UpdatedAtUtc)
            OUTPUT INSERTED.Id;";

        await using var conn = _transactionDBUtility.GetSqlConnection1(); // KhanoumiCore
        await conn.OpenAsync(ct);

        try
        {
            var insertedId = await conn.ExecuteScalarAsync<Guid?>(
                new CommandDefinition(sql, giftCard, cancellationToken: ct));

            return insertedId.HasValue; // اگر insert شد true برمی‌گرداند
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208) // Invalid object name - جدول وجود ندارد
        {
            // اگر جدول وجود نداشت، نمی‌توانیم insert کنیم
            // خطای 208 یعنی جدول یا view در دیتابیس یافت نشد
            // ممکن است جدول در دیتابیس دیگری باشد یا connection string اشتباه باشد
            var errorMessage = $"خطا در دسترسی به جدول [dbo].[GiftCard]. " +
                              $"SQL Error: {ex.Message}. " +
                              $"لطفاً مطمئن شوید که جدول در دیتابیس KhanoumiCore (TransactionDB1) وجود دارد. " +
                              $"اگر جدول در دیتابیس دیگری است، لطفاً connection string را بررسی کنید.";
            throw new InvalidOperationException(errorMessage, ex);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2627) // Unique constraint violation
        {
            // اگر unique constraint خطا داد، یعنی کاربر قبلاً کارت دریافت کرده
            return false;
        }
    }

    public async Task<bool> IsUserEligibleForGiftCardAsync(Guid userId, CancellationToken ct)
    {
        // استفاده از TransactionDB1 که به KhanoumiCore اشاره می‌کند
        // چک می‌کند که آیا NationalCode کاربر در جدول GiftCardEligibleUsers وجود دارد
        const string sql = @"
                SELECT CASE WHEN EXISTS(
                    SELECT 1 
                    FROM [dbo].[GiftCardEligibleUsers] GCEU
                    INNER JOIN [dbo].[User] U ON U.NationalCode = GCEU.NationalCode
                    WHERE U.Id = @UserId
                        AND U.NationalCode IS NOT NULL
                        AND (U.IsDeleted = 0 OR U.IsDeleted IS NULL)
                ) THEN 1 ELSE 0 END;";

        await using var conn = _transactionDBUtility.GetSqlConnection1();
        await conn.OpenAsync(ct);

        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
    }

    public async Task UpdateOrderIdAsync(Guid userId, Guid orderId, CancellationToken ct)
    {
        const string sql = @"
            UPDATE [dbo].[GiftCard]
            SET OrderId = @OrderId,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE UserId = @UserId
                AND (OrderId IS NULL OR OrderId != @OrderId);";

        await using var conn = _transactionDBUtility.GetSqlConnection1(); 
        await conn.OpenAsync(ct);

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, OrderId = orderId }, cancellationToken: ct));
    }

    public async Task UpdateAsReceivedAsync(Guid userId, Guid? orderId, CancellationToken ct)
    {
        const string sql = @"
            UPDATE [dbo].[GiftCard]
            SET IsReceived = 1,
                OrderId = @OrderId,
                ReceivedAtUtc = SYSUTCDATETIME(),
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE UserId = @UserId
                AND IsReceived = 0;";

        await using var conn = _transactionDBUtility.GetSqlConnection1(); 
        await conn.OpenAsync(ct);

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, OrderId = orderId }, cancellationToken: ct));
    }

    public async Task UpdateAsPendingAsync(Guid userId, bool isPending, CancellationToken ct)
    {
        const string sql = @"
            UPDATE [dbo].[GiftCard]
            SET IsPending = @IsPending,
                UpdatedAtUtc = SYSUTCDATETIME()
            WHERE UserId = @UserId;";

        await using var conn = _transactionDBUtility.GetSqlConnection1(); 
        await conn.OpenAsync(ct);

        await conn.ExecuteAsync(
            new CommandDefinition(sql, new { UserId = userId, IsPending = isPending }, cancellationToken: ct));
    }
}





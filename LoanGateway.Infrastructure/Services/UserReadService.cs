// src/LoanGateway.Infrastructure/Services/UserReadService.cs

using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Command.User;
using Microsoft.Extensions.Logging;

namespace LoanService.Infrastructure.Services;

public sealed class UserReadService : IUserReadService
{
    private readonly TransactionDBUtility _transactionDBUtility;
    private readonly ILogger<UserReadService> _logger;

    public UserReadService(
        TransactionDBUtility transactionDBUtility,
        ILogger<UserReadService> logger)
    {
        _transactionDBUtility = transactionDBUtility;
        _logger = logger;
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            // استفاده از TransactionDB1 که به KhanoumiCore اشاره می‌کند (همان دیتابیس Auth)
            const string sql = @"
                SELECT TOP(1) 
                    Id as UserId,
                    NationalCode,
                    PostalCode,
                    BirthDate,
                    MobileNumber,
                    IsActive,
                    GiftStatus
                FROM [dbo].[User] 
                WHERE Id = @UserId 
                    AND (IsDeleted = 0 OR IsDeleted IS NULL)";

            await using var conn = _transactionDBUtility.GetSqlConnection1();
            await conn.OpenAsync(ct);

            var user = await conn.QueryFirstOrDefaultAsync<UserInfoDto>(
                new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Id: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserCommand?> UpdateUserAsync(Guid UserId, string PostalCode, CancellationToken ct)
    {
        try
        {
            // ابتدا بررسی می‌کنیم که کاربر وجود دارد یا نه
            var user = await GetUserByIdAsync(UserId, ct);
            if (user == null)
            {
                _logger.LogWarning("User not found for postal code update. UserId: {UserId}", UserId);
                return null;
            }

            // آپدیت کد پستی
            const string updateSql = @"
                UPDATE [dbo].[User]
                SET PostalCode = @PostalCode,
                    UpdatedAtUtc = SYSUTCDATETIME()
                WHERE Id = @UserId 
                    AND (IsDeleted = 0 OR IsDeleted IS NULL)";

            await using var conn = _transactionDBUtility.GetSqlConnection1();
            await conn.OpenAsync(ct);

            var rowsAffected = await conn.ExecuteAsync(
                new CommandDefinition(updateSql, new { PostalCode = PostalCode, UserId = UserId }, cancellationToken: ct));

            if (rowsAffected == 0)
            {
                _logger.LogWarning("No rows updated for user {UserId}", UserId);
                return null;
            }

            // خواندن اطلاعات به‌روز شده کاربر
            const string selectSql = @"
                SELECT TOP(1) 
                    Id as UserId,
                    NationalCode,
                    PostalCode,
                    BirthDate,
                    MobileNumber,
                    IsActive,
                    GiftStatus
                FROM [dbo].[User] 
                WHERE Id = @UserId 
                    AND (IsDeleted = 0 OR IsDeleted IS NULL)";

            var updatedUser = await conn.QueryFirstOrDefaultAsync<UserInfoDto>(
                new CommandDefinition(selectSql, new { UserId = UserId }, cancellationToken: ct));

            if (updatedUser == null)
            {
                _logger.LogWarning("User not found after update. UserId: {UserId}", UserId);
                return null;
            }

            // تبدیل UserInfoDto به UserCommand
            return new UserCommand
            {
                UserId = updatedUser.UserId,
                NationalCode = updatedUser.NationalCode ?? string.Empty,
                BirthDate = updatedUser.BirthDate ?? string.Empty,
                PostalCode = updatedUser.PostalCode,
                MobileNumber = updatedUser.MobileNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating postal code for user {UserId}", UserId);
            throw;
        }
    }
}









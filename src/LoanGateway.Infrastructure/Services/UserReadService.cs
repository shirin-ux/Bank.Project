using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Command.User;
using LoanService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanService.Infrastructure.Services;

public sealed class UserReadService : IUserReadService
{
    private readonly AuthDbContext _context;
    private readonly ILogger<UserReadService> _logger;

    public UserReadService(
        AuthDbContext context,
        ILogger<UserReadService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserInfoDto
                {
                    UserId = u.Id,
                    NationalCode = u.NationalCode,
                    PostalCode = u.PostalCode,
                    BirthDate = u.BirthDate,
                    MobileNumber = u.MobileNumber,
                    IsActive = u.IsActive,
                    GiftStatus = u.GiftStatus ?? false
                })
                .FirstOrDefaultAsync(ct);

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
            var existingUser = await _context.Users.FindAsync(new object[] { UserId }, ct);
            if (existingUser != null)
            {
                existingUser.PostalCode = PostalCode;
               // existingUser.UpdatedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }

            // خواندن اطلاعات به‌روز شده کاربر
            var updatedUser = await GetUserByIdAsync(UserId, ct);

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

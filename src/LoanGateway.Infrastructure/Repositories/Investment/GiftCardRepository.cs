using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.IRepository.Investment;
using LoanService.Infrastructure.Persistence;
using LoanService.Domain;

namespace LoanService.Infrastructure.Repositories.Investment;

public sealed class GiftCardRepository : IGiftCardRepository
{
    private readonly AuthDbContext _context;
    private readonly AuthUnitOfWork _unitOfWork;

    public GiftCardRepository(AuthDbContext context, AuthUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<GiftCard?> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            return await _context.GiftCards
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            // اگر جدول GiftCard وجود نداشت، null برمی‌گرداند
            return null;
        }
    }

    public async Task<bool> HasUserReceivedGiftAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            return await _context.GiftCards
                .AnyAsync(x => x.UserId == userId && x.IsReceived, ct);
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            // اگر جدول GiftCard وجود نداشت، یعنی کاربر هنوز کارت دریافت نکرده است
            return false;
        }
    }

    public async Task<GiftCard> CreateAsync(GiftCard giftCard, CancellationToken ct)
    {
        try
        {
            // اگر Id set نشده باشد، یک GUID جدید ایجاد می‌کنیم
            if (giftCard.Id == Guid.Empty)
            {
                giftCard.Id = Guid.NewGuid();
            }

            _context.GiftCards.Add(giftCard);
            await _unitOfWork.SaveChangesAsync(ct);
            return giftCard;
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name
        {
            throw new InvalidOperationException(
                "جدول GiftCard در دیتابیس وجود ندارد. لطفاً اسکریپت CreateGiftCardTable.sql را اجرا کنید.", ex);
        }
    }

    public async Task<bool> TryCreateIfNotExistsAsync(GiftCard giftCard, CancellationToken ct)
    {
        try
        {
            // چک می‌کنیم که آیا برای این UserId قبلاً کارت هدیه ایجاد شده یا نه
            var existing = await _context.GiftCards
                .FirstOrDefaultAsync(x => x.UserId == giftCard.UserId, ct);

            if (existing != null)
            {
                // اگر وجود داشت، false برمی‌گردانیم (کاربر قبلاً کارت دریافت کرده)
                return false;
            }

            // اگر وجود نداشت، ایجاد می‌کنیم
            if (giftCard.Id == Guid.Empty)
            {
                giftCard.Id = Guid.NewGuid();
            }

            _context.GiftCards.Add(giftCard);
            await _unitOfWork.SaveChangesAsync(ct);
            return true; // اگر insert شد true برمی‌گرداند
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name - جدول وجود ندارد
        {
            // اگر جدول وجود نداشت، نمی‌توانیم insert کنیم
            var errorMessage = $"خطا در دسترسی به جدول [dbo].[GiftCard]. " +
                              $"SQL Error: {ex.Message}. " +
                              $"لطفاً مطمئن شوید که جدول در دیتابیس KhanoumiCore (TransactionDB1) وجود دارد. " +
                              $"اگر جدول در دیتابیس دیگری است، لطفاً connection string را بررسی کنید.";
            throw new InvalidOperationException(errorMessage, ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627) // Unique constraint violation
        {
            // اگر unique constraint خطا داد، یعنی کاربر قبلاً کارت دریافت کرده
            return false;
        }
    }

    public async Task<bool> IsUserEligibleForGiftCardAsync(Guid userId, CancellationToken ct)
    {
        // چک می‌کند که آیا NationalCode کاربر در جدول GiftCardEligibleUsers وجود دارد
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.NationalCode != null, ct);

            if (user == null || string.IsNullOrEmpty(user.NationalCode))
                return false;

            // استفاده از LINQ برای چک کردن GiftCardEligibleUsers
            return await _context.GiftCardEligibleUsers
                .AnyAsync(gceu => gceu.NationalCode == user.NationalCode, ct);
        }
        catch
        {
            // اگر جدول GiftCardEligibleUsers وجود نداشت، false برمی‌گردانیم
            return false;
        }
    }

    public async Task UpdateOrderIdAsync(Guid userId, Guid orderId, CancellationToken ct)
    {
        var giftCard = await _context.GiftCards
            .FirstOrDefaultAsync(x => x.UserId == userId && (x.OrderId == null || x.OrderId != orderId), ct);

        if (giftCard != null)
        {
            giftCard.OrderId = orderId;
            giftCard.UpdatedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateAsReceivedAsync(Guid userId, Guid? orderId, CancellationToken ct)
    {
        var giftCard = await _context.GiftCards
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsReceived, ct);

        if (giftCard != null)
        {
            giftCard.IsReceived = true;
            giftCard.OrderId = orderId;
            giftCard.ReceivedAtUtc = DateTime.UtcNow;
            giftCard.UpdatedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateAsPendingAsync(Guid userId, bool isPending, CancellationToken ct)
    {
        var giftCard = await _context.GiftCards
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (giftCard != null)
        {
            giftCard.IsPending = isPending;
            giftCard.UpdatedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using LoanGateway.Auth.Domain;

namespace LoanGateway.Auth.Infrastructure.Repositories;

public sealed class UserReadRepository : IUserReadRepository
{
    private readonly AuthDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UserReadRepository(AuthDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ExistsByMobileOrNationalCodeAsync(string mobileNumber, string nationalCode, CancellationToken ct)
    {
        // Note: IsDeleted might not exist in database, so we check without it
        return await _context.Users
            .AnyAsync(x => x.MobileNumber == mobileNumber || x.NationalCode == nationalCode, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }
        catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
        {
            throw;
            // Fallback if GiftStatus column doesn't exist
            //return await _context.Users
            //    .Where(x => x.Id == id)
            //    .Select(x => new User
            //    {
            //        Id = x.Id,
            //        MobileNumber = x.MobileNumber,
            //        NationalCode = x.NationalCode,
            //        PostalCode = x.PostalCode,
            //        FirstName = x.FirstName,
            //        LastName = x.LastName,
            //        IsMobileVerified = x.IsMobileVerified,
            //        IsProfileCompleted = x.IsProfileCompleted,
            //       // KycLevel = x.KycLevel,
            //        IsActive = x.IsActive,
            //        ShenasnameSerial = x.ShenasnameSerial,
            //        ShenasnameSeri = x.ShenasnameSeri,
            //        ShenasnamehNumber = x.ShenasnamehNumber,
            //       // LastLoginAtUtc = x.LastLoginAtUtc,
            //       // IsKycVerified = x.IsKycVerified,
            //        BirthDate = x.BirthDate,
            //       // CreatedAtUtc = x.CreatedAtUtc,
            //       // UpdatedAtUtc = x.UpdatedAtUtc,
            //        GiftStatus = false
            //    })
            //    .FirstOrDefaultAsync(ct);
        }
    }

    public async Task<User?> GetByMobileAsync(string mobileNumber, CancellationToken ct)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber, ct);
        }
        catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
        {
            throw;
            //return await _context.Users
            //    .Where(x => x.MobileNumber == mobileNumber)
            //    .Select(x => new User
            //    {
            //        Id = x.Id,
            //        MobileNumber = x.MobileNumber,
            //        NationalCode = x.NationalCode,
            //        PostalCode = x.PostalCode,
            //        FirstName = x.FirstName,
            //        LastName = x.LastName,
            //        IsMobileVerified = x.IsMobileVerified,
            //        IsProfileCompleted = x.IsProfileCompleted,
            //       // KycLevel = x.KycLevel,
            //        IsActive = x.IsActive,
            //        ShenasnameSerial = x.ShenasnameSerial,
            //        ShenasnameSeri = x.ShenasnameSeri,
            //        ShenasnamehNumber = x.ShenasnamehNumber,
            //       // LastLoginAtUtc = x.LastLoginAtUtc,
            //       // IsKycVerified = x.IsKycVerified,
            //        BirthDate = x.BirthDate,
            //      //  CreatedAtUtc = x.CreatedAtUtc,
            //      //  UpdatedAtUtc = x.UpdatedAtUtc,
            //        GiftStatus = false
            //    })
            //    .FirstOrDefaultAsync(ct);
        }
    }

    public async Task<User?> GetByNationalCodeAsync(string nationalCode, CancellationToken ct)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.NationalCode == nationalCode, ct);
        }
        catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
        {
            throw;
            //return await _context.Users
            //    .Where(x => x.NationalCode == nationalCode)
            //    .Select(x => new User
            //    {
            //        Id = x.Id,
            //        MobileNumber = x.MobileNumber,
            //        NationalCode = x.NationalCode,
            //        PostalCode = x.PostalCode,
            //        FirstName = x.FirstName,
            //        LastName = x.LastName,
            //        IsMobileVerified = x.IsMobileVerified,
            //        IsProfileCompleted = x.IsProfileCompleted,
            //        //KycLevel = x.KycLevel,
            //        IsActive = x.IsActive,
            //        ShenasnameSerial = x.ShenasnameSerial,
            //        ShenasnameSeri = x.ShenasnameSeri,
            //        ShenasnamehNumber = x.ShenasnamehNumber,
            //        //LastLoginAtUtc = x.LastLoginAtUtc,
            //       // IsKycVerified = x.IsKycVerified,
            //        BirthDate = x.BirthDate,
            //        //CreatedAtUtc = x.CreatedAtUtc,
            //        //UpdatedAtUtc = x.UpdatedAtUtc,
            //        GiftStatus = false
            //    })
            //    .FirstOrDefaultAsync(ct);
        }
    }

    public async Task<User?> GetUserAsync(CancellationToken ct)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(ct);
        }
        catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
        {
            throw;
            //    return await _context.Users
            //        .Select(x => new User
            //        {
            //            Id = x.Id,
            //            MobileNumber = x.MobileNumber,
            //            NationalCode = x.NationalCode,
            //            PostalCode = x.PostalCode,
            //            FirstName = x.FirstName,
            //            LastName = x.LastName,
            //            IsMobileVerified = x.IsMobileVerified,
            //            IsProfileCompleted = x.IsProfileCompleted,
            //            //KycLevel = x.KycLevel,
            //            IsActive = x.IsActive,
            //            ShenasnameSerial = x.ShenasnameSerial,
            //            ShenasnameSeri = x.ShenasnameSeri,
            //            ShenasnamehNumber = x.ShenasnamehNumber,
            //            //LastLoginAtUtc = x.LastLoginAtUtc,
            //            //IsKycVerified = x.IsKycVerified,
            //            BirthDate = x.BirthDate,
            //            //CreatedAtUtc = x.CreatedAtUtc,
            //            //UpdatedAtUtc = x.UpdatedAtUtc,
            //            GiftStatus = false
            //        })
            //        .FirstOrDefaultAsync(ct);
        }
    }

    public async Task<User> InsertAsync(User user, CancellationToken ct)
    {
        try
        {
            if (user.Id == Guid.Empty)
                user.Id = Guid.NewGuid();

            _context.Users.Add(user);
            await _unitOfWork.SaveChangesAsync(ct);
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
            var existing = await _context.Users.FindAsync(new object[] { user.Id }, ct);
            if (existing != null)
            {
                existing.PostalCode = user.PostalCode;
               // existing.UpdatedAtUtc = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}

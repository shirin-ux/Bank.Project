using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using LoanGateway.Auth.Domain;

namespace LoanGateway.Auth.Infrastructure.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly AuthDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UserProfileRepository(AuthDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<User?> GetByNationalCodeAsync(string nationalCode, CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.NationalCode == nationalCode, ct);
    }

    public async Task<bool> ExistsByNationalCodeAsync(
        string nationalCode,
        string mobileNumber,
        Guid? excludeUserId,
        CancellationToken ct)
    {
        return await _context.Users
            .AnyAsync(x => x.NationalCode == nationalCode &&
                          x.MobileNumber != mobileNumber &&
                          (excludeUserId == null || x.Id != excludeUserId), ct);
    }

    public async Task UpdateProfileAfterKycAsync(User user, CancellationToken ct)
    {
        try
        {
            var existing = await _context.Users.FindAsync(new object[] { user.Id }, ct);
            if (existing != null)
            {
                existing.ShenasnamehNumber = user.ShenasnamehNumber;
                existing.ShenasnameSeri = user.ShenasnameSeri;
                existing.ShenasnameSerial = user.ShenasnameSerial;
                existing.PostalCode = user.PostalCode;
                existing.NationalCode = user.NationalCode;
                existing.FirstName = user.FirstName;
                existing.LastName = user.LastName;
                existing.BirthDate = user.BirthDate;
                existing.IsProfileCompleted = user.IsProfileCompleted;
              //  existing.IsKycVerified = user.IsKycVerified;
                existing.GiftStatus = user.GiftStatus ?? false;
              //  existing.UpdatedAtUtc = user.UpdatedAtUtc;

                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
        catch (SqlException ex) when (ex.Message.Contains("GiftStatus") || ex.Number == 207)
        {
            // Fallback if GiftStatus column doesn't exist
            var existing = await _context.Users.FindAsync(new object[] { user.Id }, ct);
            if (existing != null)
            {
                existing.ShenasnamehNumber = user.ShenasnamehNumber;
                existing.ShenasnameSeri = user.ShenasnameSeri;
                existing.ShenasnameSerial = user.ShenasnameSerial;
                existing.PostalCode = user.PostalCode;
                existing.NationalCode = user.NationalCode;
                existing.FirstName = user.FirstName;
                existing.LastName = user.LastName;
                existing.BirthDate = user.BirthDate;
                existing.IsProfileCompleted = user.IsProfileCompleted;
                //existing.IsKycVerified = user.IsKycVerified;
              //  existing.UpdatedAtUtc = user.UpdatedAtUtc;

                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.Enum;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using LoanGateway.Auth.Domain;

namespace LoanGateway.Auth.Infrastructure.Repositories;

public sealed class UserOtpRepository : IUserOtpRepository
{
    private readonly AuthDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UserOtpRepository(AuthDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CountRequestsInWindowAsync(string phoneNumber, OtpPurpose purpose, DateTime utcFrom, CancellationToken ct = default)
    {
        try
        {
            return await _context.OtpCodes
                .CountAsync(x => x.PhoneNumber == phoneNumber &&
                                x.Purpose == purpose &&
                                x.CreatedAtUtc >= utcFrom &&
                                (x.IsDeleted == null || x.IsDeleted == false), ct);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<int> CountRequestsByIpInWindowAsync(
        string requestIp,
        DateTime utcFrom,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(requestIp))
            {
                return 0;
            }

            return await _context.OtpCodes
                .CountAsync(x => x.RequestIp == requestIp &&
                                x.CreatedAtUtc >= utcFrom &&
                                (x.IsDeleted == null || x.IsDeleted == false), ct);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<OtpCode> GetActiveAsync(string phoneNumber, OtpPurpose purpose, DateTime nowUtc, CancellationToken ct = default)
    {
        try
        {
            return await _context.OtpCodes
                .Where(x => x.PhoneNumber == phoneNumber &&
                           x.Purpose == purpose &&
                           (x.IsDeleted == null || x.IsDeleted == false) &&
                           x.ConsumedAtUtc == null &&
                           x.ExpiresAtUtc > nowUtc)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task InsertAsync(OtpCode otp, CancellationToken ct = default)
    {
        _context.OtpCodes.Add(otp);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkConsumedAsync(Guid id, DateTime consumedAtUtc, CancellationToken ct)
    {
        var otp = await _context.OtpCodes.FindAsync(new object[] { id }, ct);
        if (otp != null)
        {
            otp.ConsumedAtUtc = consumedAtUtc;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateAsync(OtpCode otp, CancellationToken ct = default)
    {
        var existing = await _context.OtpCodes.FindAsync(new object[] { otp.Id }, ct);
        if (existing != null)
        {
            existing.ConsumedAtUtc = otp.ConsumedAtUtc;
            existing.IsDeleted = otp.IsDeleted;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateFailedAttemptsAsync(Guid id, int failedAttempts, CancellationToken ct)
    {
        var otp = await _context.OtpCodes.FindAsync(new object[] { id }, ct);
        if (otp != null)
        {
            otp.FailedAttempts = failedAttempts;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    public bool Verify(string code, string phoneNumber, int purpose, byte[] storedHash)
    {
        throw new NotImplementedException();
    }
}

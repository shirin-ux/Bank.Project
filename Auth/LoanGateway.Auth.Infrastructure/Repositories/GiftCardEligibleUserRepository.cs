using Microsoft.EntityFrameworkCore;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using LoanGateway.Auth.Domain;
using LoanGateway.Auth.Domain.Entities;

namespace LoanGateway.Auth.Infrastructure.Repositories;

public sealed class GiftCardEligibleUserRepository : IGiftCardEligibleUserRepository
{
    private readonly AuthDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public GiftCardEligibleUserRepository(AuthDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsEligibleByNationalCodeAsync(string nationalCode, CancellationToken ct)
    {
        return await _context.GiftCardEligibleUsers
            .AnyAsync(x => x.NationalCode == nationalCode, ct);
    }

    public async Task MarkAsProcessedAsync(string nationalCode, CancellationToken ct)
    {
        var eligibleUser = await _context.GiftCardEligibleUsers
            .FirstOrDefaultAsync(x => x.NationalCode == nationalCode, ct);

        if (eligibleUser != null)
        {
            eligibleUser.IsProcessed = true;
            eligibleUser.ProcessedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}

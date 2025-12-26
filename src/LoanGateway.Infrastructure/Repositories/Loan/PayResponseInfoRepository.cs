using Microsoft.EntityFrameworkCore;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.IRepository.Loan;
using LoanService.Infrastructure.Persistence;
using LoanService.Domain;

namespace LoanService.Infrastructure.Repositories.Loan;

public class PayResponseInfoRepository : IPayResponseInfoRepository
{
    private readonly LoanDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PayResponseInfoRepository(LoanDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> InsertAsync(PayResponseInfo entity, CancellationToken ct)
    {
        _context.Set<PayResponseInfo>().Add(entity);
        return await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(PayResponseInfo entity, CancellationToken ct)
    {
        _context.Set<PayResponseInfo>().Update(entity);
        return await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PayResponseInfo?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Set<PayResponseInfo>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<PayResponseInfo>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Set<PayResponseInfo>()
            .OrderByDescending(x => x.ReceivedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Set<PayResponseInfo>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        
        if (entity != null)
        {
            _context.Set<PayResponseInfo>().Remove(entity);
            return await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return 0;
    }
}

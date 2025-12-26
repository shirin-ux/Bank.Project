using Microsoft.EntityFrameworkCore;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.IRepository.Loan;
using LoanService.Infrastructure.Persistence;
using LoanService.Domain;

namespace LoanService.Infrastructure.Repositories.Loan;

public class InstallmentRepository : IInstallmentRepository
{
    private readonly LoanDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public InstallmentRepository(LoanDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> InsertAsync(InstallmentStatus entity, CancellationToken ct)
    {
        _context.Set<InstallmentStatus>().Add(entity);
        return await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(InstallmentStatus entity, CancellationToken ct)
    {
        _context.Set<InstallmentStatus>().Update(entity);
        return await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<InstallmentStatus?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Set<InstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<InstallmentStatus>> GetByLoanRequestIdAsync(Guid loanRequestId, CancellationToken ct)
    {
        return await _context.Set<InstallmentStatus>()
            .Where(x => x.LoanRequestId == loanRequestId)
            .OrderBy(x => x.InstallmentNo)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<InstallmentStatus>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Set<InstallmentStatus>()
            .OrderBy(x => x.DueDate)
            .ToListAsync(ct);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Set<InstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        
        if (entity != null)
        {
            _context.Set<InstallmentStatus>().Remove(entity);
            return await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return 0;
    }
}

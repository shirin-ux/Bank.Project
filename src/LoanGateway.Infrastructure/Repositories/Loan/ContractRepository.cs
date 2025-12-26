using Microsoft.EntityFrameworkCore;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.IRepository.Loan;
using LoanService.Infrastructure.Persistence;
using LoanService.Domain;

namespace LoanService.Infrastructure.Repositories.Loan;

public class ContractRepository : IContractRepository
{
    private readonly LoanDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ContractRepository(LoanDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> InsertAsync(ContractInfo entity, CancellationToken ct)
    {
        _context.Set<ContractInfo>().Add(entity);
        return await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(ContractInfo entity, CancellationToken ct)
    {
        _context.Set<ContractInfo>().Update(entity);
        return await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<ContractInfo?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Set<ContractInfo>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<ContractInfo>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Set<ContractInfo>()
            .OrderByDescending(x => x.ContractNumber)
            .ToListAsync(ct);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Set<ContractInfo>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        
        if (entity != null)
        {
            _context.Set<ContractInfo>().Remove(entity);
            return await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return 0;
    }
}

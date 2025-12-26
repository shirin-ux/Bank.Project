using Microsoft.EntityFrameworkCore;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.IRepository.Loan;
using LoanService.Infrastructure.Persistence;
using LoanService.Domain;

namespace LoanService.Infrastructure.Repositories.Loan;

public class InquiryInfoRepository : IInquiryInfoRepository
{
    private readonly LoanDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public InquiryInfoRepository(LoanDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> InsertAsync(InquiryInfo entity, CancellationToken ct)
    {
        _context.Set<InquiryInfo>().Add(entity);
        return await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(InquiryInfo entity, CancellationToken ct)
    {
        _context.Set<InquiryInfo>().Update(entity);
        return await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<InquiryInfo?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Set<InquiryInfo>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IEnumerable<InquiryInfo>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Set<InquiryInfo>()
            .OrderByDescending(x => x.ExpireAt)
            .ToListAsync(ct);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Set<InquiryInfo>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        
        if (entity != null)
        {
            _context.Set<InquiryInfo>().Remove(entity);
            return await _unitOfWork.SaveChangesAsync(ct);
        }
        
        return 0;
    }
}

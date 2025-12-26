using LoanService.Domain.Entities.Loan;

namespace LoanService.Domain.IRepository.Loan;

public interface IInquiryInfoRepository
{
    Task<int> InsertAsync(InquiryInfo entity, CancellationToken ct);
    Task<int> UpdateAsync(InquiryInfo entity, CancellationToken ct);
    Task<InquiryInfo?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<InquiryInfo>> GetAllAsync(CancellationToken ct);
}


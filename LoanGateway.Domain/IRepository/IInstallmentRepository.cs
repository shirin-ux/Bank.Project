using LoanService.Domain.Entities;

namespace LoanService.Domain.IRepository
{
   public interface IInstallmentRepository
    {
        Task<int> InsertAsync(InstallmentStatus entity,CancellationToken ct);
        Task<int> UpdateAsync(InstallmentStatus entity, CancellationToken ct);
        Task<InstallmentStatus?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<InstallmentStatus>> GetByLoanRequestIdAsync(Guid loanRequestId, CancellationToken ct);
        Task<IEnumerable<InstallmentStatus>> GetAllAsync(CancellationToken ct);
        Task<int> DeleteAsync(Guid id, CancellationToken ct);
    }
}

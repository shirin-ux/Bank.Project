using LoanService.Domain.Entities.Loan;

namespace LoanService.Domain.IRepository.Loan
{
   public interface ILoanRequestRepository
    {
        Task InsertAsync(LoanRequest loan, CancellationToken ct);
        Task<LoanRequest?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<LoanRequest?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct);
        Task UpdateAsync(LoanRequest loan, CancellationToken ct);

        Task InsertInstallmentAsync(List<InstallmentStatus> installmentStatus, Guid loanId, CancellationToken ct);

    }
}

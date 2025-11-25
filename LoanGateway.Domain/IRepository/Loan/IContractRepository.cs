using LoanService.Domain.Entities.Loan;

namespace LoanService.Domain.IRepository.Loan;

public interface IContractRepository
{
    Task<int> InsertAsync(ContractInfo entity, CancellationToken ct);
    Task<int> UpdateAsync(ContractInfo entity, CancellationToken ct);
    Task<ContractInfo?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<ContractInfo>> GetAllAsync(CancellationToken ct);
    Task<int> DeleteAsync(Guid id, CancellationToken ct);
}


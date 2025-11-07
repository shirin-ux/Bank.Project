using LoanService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities;

   public interface IContractRepository
    {
        Task<int> InsertAsync(ContractInfo entity, CancellationToken ct);
        Task<int> UpdateAsync(ContractInfo entity, CancellationToken ct);
        Task<ContractInfo?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<ContractInfo>> GetAllAsync(CancellationToken ct);
    Task<int> DeleteAsync(Guid id, CancellationToken ct);
    }


using LoanService.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities;

   public interface IPayResponseInfoRepository
{
        Task<int> InsertAsync(PayResponseInfo entity, CancellationToken ct);
        Task<int> UpdateAsync(PayResponseInfo entity, CancellationToken ct);
        Task<PayResponseInfo?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<PayResponseInfo>> GetAllAsync(CancellationToken ct);
    Task<int> DeleteAsync(Guid id, CancellationToken ct);
    }


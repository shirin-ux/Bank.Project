using LoanService.Domain.Entities.Loan;
using LoanService.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities;

   public interface IInquiryInfoRepository
{
        Task<int> InsertAsync(InquiryInfo entity,CancellationToken ct);
        Task<int> UpdateAsync(InquiryInfo entity, CancellationToken ct);
        Task<InquiryInfo?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<InquiryInfo>> GetAllAsync( CancellationToken ct);
    }


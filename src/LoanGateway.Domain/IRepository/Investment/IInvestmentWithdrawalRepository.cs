using LoanService.Domain.Entities.Investment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.IRepository.Investment
{
    public interface IInvestmentWithdrawalRepository
    {
        void Add(InvestmentWithdrawal withdrawal);
        Task<InvestmentWithdrawal?> GetByIdAsync(Guid id, CancellationToken ct);
    }
}

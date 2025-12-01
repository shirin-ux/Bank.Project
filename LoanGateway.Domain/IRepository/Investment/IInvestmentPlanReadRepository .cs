using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Domain.IRepository.Investment
{
    public interface IInvestmentPlanReadRepository
    {
        Task<IEnumerable<InvestmentPlans>> GetActivePlansAsync(CancellationToken ct);
        Task<InvestmentPlans> GetByIdAsync(Guid Id, CancellationToken ct);
        Task<InvestmentPlans> GetByCodeAsync(string Code, CancellationToken ct);
        Task<Guid> InsertAsync(InvestmentPlans plan, CancellationToken ct);
        Task<bool> UpdateAsync(InvestmentPlans plan, CancellationToken ct);
        Task<bool> DeletAsync(InvestmentPlans plan, CancellationToken ct);
        Task<InvestmentPlans?> GetPlanWithMetaAsync(InvestmentPlanType planType, CancellationToken ct);
    }
}

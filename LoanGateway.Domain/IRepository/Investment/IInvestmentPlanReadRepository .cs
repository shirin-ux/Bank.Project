using LoanService.Domain.Entities.Investment;

namespace LoanService.Domain.IRepository.Investment
{
    public interface IInvestmentPlanReadRepository
    {
        Task<IEnumerable<InvestmentPlan>> GetActivePlansAsync(CancellationToken ct);
        Task<InvestmentPlan> GetByIdAsync(Guid Id, CancellationToken ct);
        Task<InvestmentPlan> GetByCodeAsync(string Code, CancellationToken ct);
        Task<Guid> InsertAsync(InvestmentPlan plan, CancellationToken ct);
        Task<bool> UpdateAsync(InvestmentPlan plan, CancellationToken ct);
        Task<bool> DeletAsync(InvestmentPlan plan, CancellationToken ct);

    }
}

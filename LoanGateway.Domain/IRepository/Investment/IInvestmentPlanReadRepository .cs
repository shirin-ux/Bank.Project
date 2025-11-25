using LoanService.Domain.Entities.Investment;

namespace LoanService.Domain.IRepository.Investment
{
    public interface IInvestmentPlanReadRepository
    {
       Task<IEnumerable<InvestmentPlan>> GetActivePlansAsync(CancellationToken ct);
    }
}

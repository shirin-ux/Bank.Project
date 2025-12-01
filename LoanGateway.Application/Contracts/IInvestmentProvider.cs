using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.Contracts
{
    public interface IInvestmentProvider
    {
        Task<PlanSnapshotDto> GetPlanSnapshotAsync(InvestmentPlanType plan, CancellationToken ct);
        Task<IReadOnlyList<IndexPointDto>> GetPlanIndexHistoryAsync(InvestmentPlanType plan, InvestmentChartRange range, CancellationToken ct);
    }
}

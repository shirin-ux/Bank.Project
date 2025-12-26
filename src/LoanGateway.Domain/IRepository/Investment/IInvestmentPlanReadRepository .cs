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

        Task UpsertDailyHistoryAsync(InvestmentPlanType plan, IReadOnlyList<InvestmentIndexHistory> points, CancellationToken ct);

        Task<IReadOnlyList<InvestmentIndexHistory>> GetRangeAsync(InvestmentPlanType plan, DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct);

        Task<InvestmentAccount?> GetByNationalCodeAndPlanAsync(string nationalCode,InvestmentPlanType planType, CancellationToken ct);

        Task AddAsync(InvestmentAccount account, CancellationToken ct);
        Task<bool> UpdateAsync(InvestmentAccount plan, CancellationToken ct);

        Task<DateTime?> GetLastDateAsync(
            InvestmentPlanType plan, CancellationToken ct);

        Task DeleteOlderThanAsync(
            InvestmentPlanType plan, DateTime cutoffUtc, CancellationToken ct);
        Task<InvestmentPlans> GetPlanAsync(string title, CancellationToken ct);

        Task<List<InvestmentIndexHistory>> GetLatestPointsAsync(InvestmentPlanType planType,int count, CancellationToken ct);

        /// <summary>
        /// بررسی می‌کند که آیا operation با receiptNumber مشخص برای PolicyId مشخص وجود دارد یا نه
        /// </summary>
        Task<bool> CheckReceiptNumberExistsAsync(Guid? policyId, string receiptNumber, CancellationToken ct);

    }
}

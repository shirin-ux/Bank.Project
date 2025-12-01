using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Domain.Enum.Investment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts
{
    public interface IInvestmentProvider
    {
        Task<PlanSnapshotDto> GetPlanSnapshotAsync(InvestmentPlanType plan, CancellationToken ct);
        Task<IReadOnlyList<IndexPointDto>> GetPlanIndexHistoryAsync( InvestmentPlanType plan,InvestmentChartRange range,CancellationToken ct);
    }
}

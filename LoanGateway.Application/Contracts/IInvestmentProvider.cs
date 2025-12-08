using LoanService.Application.UseCase.Investment.Command.BuyPlanCommand;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Application.UseCase.Investment.Query.PlanBuyInfo;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.Contracts
{
    public interface IInvestmentProvider
    {
        Task<IReadOnlyList<IndexPointDto>> GetPlanIndexHistoryAsync(InvestmentPlanType plan, InvestmentChartRange range,CancellationToken ct, bool forceRefresh);
        Task<PlanPriceInfoDto> GetCurrentPriceAsync( InvestmentPlanType planType, CancellationToken ct);

        Task<BuyPlanResultDto> CreatePolicyAndBuyAsync(BuyPlanCommand cmd, CancellationToken ct);
        Task<BuyPlanResultDto> GetRevokableAmountAsync(BuyPlanCommand cmd, CancellationToken ct);
    }
}

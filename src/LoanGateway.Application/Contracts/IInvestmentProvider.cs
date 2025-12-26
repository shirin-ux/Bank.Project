using Common;
using LoanService.Application.UseCase.Investment.Command.BuyPlan;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Application.UseCase.Investment.Query.OrderBuy;
using LoanService.Application.UseCase.Investment.Query.PlanBuyInfo;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.Contracts
{
    public interface IInvestmentProvider
    {
        Task<IReadOnlyList<IndexPointDto>> GetPlanIndexHistoryAsync(InvestmentPlanType plan, InvestmentChartRange range,CancellationToken ct,  bool forceRefresh, bool isMinute = false);
        Task<PlanPriceInfoDto> GetCurrentPriceAsync( InvestmentPlanType planType, CancellationToken ct);

        Task<Result<BuyPlanResultDto>> CreatePolicyAndBuyAsync(BuyPlanCommand cmd,string birthDate,string postalCode,string nationalCode, CancellationToken ct);
        Task<BuyPlanResultDto> GetRevokableAmountAsync(BuyPlanCommand cmd, CancellationToken ct);
        Task<GetOrderStatusResultDto> GetOrderBuyByIdAsync(Guid Id, CancellationToken ct);
        Task<Result<IncreaseCapitalDirectResultDto>> IncreaseCapitalDirectAsync(long wealthPolicyId, decimal amount, string receiptNumber, DateTime receiptDate, string? description, CancellationToken ct);
    }
    
    public class IncreaseCapitalDirectResultDto
    {
        public long TraceId { get; set; }
        public Guid OrderId { get; set; }
        public bool IsRepeated { get; set; }
    }
}

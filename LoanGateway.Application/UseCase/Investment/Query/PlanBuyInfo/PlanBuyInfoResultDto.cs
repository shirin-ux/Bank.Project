using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.UseCase.Investment.Query.PlanBuyInfo
{
    public sealed class PlanBuyInfoResultDto
    {
        public InvestmentPlanType PlanType { get; set; }
        public string PlanTitle { get; set; } = default!;

        public decimal MinAmount { get; set; }
        public decimal? MinAmountEquivalentGram { get; set; }

        public decimal? GramPrice { get; set; }
        public decimal? DailyChangePercent { get; set; }

        public string Description { get; set; } = default!;
    }
}


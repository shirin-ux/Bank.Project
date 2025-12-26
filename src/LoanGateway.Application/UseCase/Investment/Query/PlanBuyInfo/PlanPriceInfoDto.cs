namespace LoanService.Application.UseCase.Investment.Query.PlanBuyInfo
{
    public sealed class PlanPriceInfoDto
    {
        public decimal CurrentPrice { get; set; }          // قیمت لحظه‌ای
        public decimal DailyChangePercent { get; set; }
        public DateTimeOffset LastUpdateUtc { get; set; }
    }
}

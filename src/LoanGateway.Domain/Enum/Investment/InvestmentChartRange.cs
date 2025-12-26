namespace LoanService.Domain.Enum.Investment
{
    public enum InvestmentChartRange
    {
        ThreeMonths = 1,
        SixMonths = 2,
        OneYear = 3,
        OneDay=4,
        OneHour=5
    }

    public static class ChartRangeExtensions
    {
        public static (DateTime From, DateTime To) ToDateRange(this InvestmentChartRange range, DateTime nowUtc)
        {
            return range switch
            {
                InvestmentChartRange.ThreeMonths => (nowUtc.AddMonths(-3), nowUtc),
                InvestmentChartRange.SixMonths => (nowUtc.AddMonths(-6), nowUtc),
                InvestmentChartRange.OneYear => (nowUtc.AddYears(-1), nowUtc),
                InvestmentChartRange.OneDay => (nowUtc.AddDays(-1), nowUtc),
                InvestmentChartRange.OneHour => (nowUtc.AddMinutes(-60), nowUtc),
                _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
            };
        }
    }
}

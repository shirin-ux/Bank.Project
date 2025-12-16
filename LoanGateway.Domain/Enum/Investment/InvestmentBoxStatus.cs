using LoanService.Domain.Exceptions;

namespace LoanService.Domain.Enum.Investment
{
    public enum InvestmentBoxStatus
    {
        Daily = 1,
        Minute = 2
    }

    public static class BoxStatusExtensions
    {
        public static (DateTime From, DateTime To) ToDateRange(this InvestmentBoxStatus boxStatus, InvestmentPlanType planType, InvestmentChartRange requestedRange, DateTime nowUtc)
        {
            if (planType == InvestmentPlanType.FixedIncome && boxStatus == InvestmentBoxStatus.Minute)
            {
                throw new LogicException("برای طرح درآمد ثابت فقط بازه روزانه مجاز است.");
            }


            return boxStatus switch
            {
                InvestmentBoxStatus.Daily => planType switch
                {
                    InvestmentPlanType.FixedIncome => requestedRange.ToDateRange(nowUtc), // 3،6،9 ماهه مدیریت شده
                    InvestmentPlanType.Gold or InvestmentPlanType.Silver => requestedRange switch
                    {
                        InvestmentChartRange.OneDay => (nowUtc.Date.AddDays(-1), nowUtc),
                        InvestmentChartRange.OneHour => (nowUtc.Date, nowUtc),
                        InvestmentChartRange.ThreeMonths => (nowUtc.AddMonths(-3), nowUtc),
                        InvestmentChartRange.SixMonths => (nowUtc.AddMonths(-6), nowUtc),
                        InvestmentChartRange.OneYear => (nowUtc.AddYears(-1), nowUtc),
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    _ => requestedRange.ToDateRange(nowUtc)
                },
                InvestmentBoxStatus.Minute => planType switch
                {
                    InvestmentPlanType.Gold or InvestmentPlanType.Silver => (nowUtc.AddMinutes(-60), nowUtc),
                    _ => throw new LogicException("برای این طرح، بازه دقیقه‌ای مجاز نیست.")
                }
            };
        }
    }
}

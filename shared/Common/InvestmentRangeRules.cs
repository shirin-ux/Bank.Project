using LoanService.Domain.Enum.Investment;
using LoanService.Domain.Exceptions;

namespace Common;

public static class InvestmentRangeRules
{
    public static bool IsAllowed(InvestmentPlanType plan, InvestmentChartRange range)
        => plan switch
        {
            InvestmentPlanType.FixedIncome
                => range is InvestmentChartRange.ThreeMonths
                        or InvestmentChartRange.SixMonths
                        or InvestmentChartRange.OneYear,

            InvestmentPlanType.Gold or InvestmentPlanType.Silver
                => range is InvestmentChartRange.OneHour
                        or InvestmentChartRange.OneDay
                        or InvestmentChartRange.ThreeMonths
                        or InvestmentChartRange.SixMonths
                        or InvestmentChartRange.OneYear,

            _ => false
        };

    public static void EnsureAllowed(InvestmentPlanType plan, InvestmentChartRange range)
    {
        if (IsAllowed(plan, range)) return;

        var msg = plan switch
        {
            InvestmentPlanType.FixedIncome =>
                "برای طرح درآمد ثابت فقط بازه‌های ۳ ماهه، ۶ ماهه و ۱ ساله مجاز است.",

            InvestmentPlanType.Gold or InvestmentPlanType.Silver =>
                "برای طرح طلا/نقره فقط بازه‌های ۱ ساعته، ۱ روزه، ۳ ماهه، ۶ ماهه و ۱ ساله مجاز است.",

            _ => $"بازه '{range}' برای طرح '{plan}' مجاز نیست."
        };

        throw new LogicException(msg);
    }
}

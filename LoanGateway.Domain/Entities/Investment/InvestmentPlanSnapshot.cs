using LoanService.Domain.Enum.Investment;

namespace LoanService.Domain.Entities.Investment;

public class InvestmentPlanSnapshot : BaseEntity
{
    /// <summary>
    /// نوع طرح (طلا، نقره، درآمد ثابت و ...)
    /// </summary>
    public InvestmentPlanType PlanType { get; set; }

    /// <summary>
    /// قیمت فعلی هر گرم شمش طلا 24 عیار 
    /// </summary>
    public decimal GramPrice { get; set; }

    /// <summary>
    /// درصد تغییر قیمت نسبت به روز گذشته (مثلاً +1.12%)
    /// </summary>
    public decimal GramPriceChangePercent { get; set; }

    /// <summary>
    /// مقدار فعلی شاخص طرح (NAV / Index) – اگر کاریزما این عدد را می‌دهد.
    /// اگر نداشتی، می‌تونی 0 بزاری یا فعلاً حذفش کنی.
    /// </summary>
    public decimal CurrentIndexValue { get; set; }

    /// <summary>
    /// درصد تغییر شاخص نسبت به روز گذشته (اختیاری)
    /// </summary>
    public decimal IndexChangePercentDaily { get; set; }

    /// <summary>
    /// سود موثر روزشمار سالانه (٪) – همون چیزی که باید در صفحه نمایش بدی.
    /// </summary>
    public decimal EffectiveAnnualRate { get; set; }

    /// <summary>
    /// درصد بازدهی از ابتدای دوره (٪) – روی شاخص محاسبه می‌شود.
    /// </summary>
    public decimal ReturnFromStartPercent { get; set; }

    /// <summary>
    /// زمان آخرین به‌روزرسانی این داده‌ها (برای نمایش "آخرین بروزرسانی")
    /// </summary>
    public DateTime LastUpdateUtc { get; set; }
}
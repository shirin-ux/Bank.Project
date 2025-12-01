using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans
{
   public class InvestmentPlanDetailsResultDto
    {
        public InvestmentPlanType PlanType { get; set; }   // Gold, Silver, FixedIncome
        public string PlanTitle { get; set; }              // "طرح سرمایه‌گذاری طلا (بیمه‌نامه)"

        // اطلاعات قیمت فعلی
        public decimal GramPrice { get; set; }             // قیمت هر گرم
        public decimal DailyChangePercent { get; set; }    // +1.12%

        // اطلاعات بازدهی
        public decimal EffectiveAnnualRate { get; set; }   // سود مؤثر روزشمار سالانه (٪)
        public decimal ReturnFromStartPercent { get; set; }// بازدهی از ابتدای دوره (٪)

        // نمودار شاخص
        public IReadOnlyList<IndexPointDto> IndexHistory { get; set; } = new List<IndexPointDto>();

        // ویژگی‌های طرح
        public IReadOnlyList<PlanFeatureDto> Features { get; set; } = new List<PlanFeatureDto>();

        // FAQ
        public IReadOnlyList<FaqItemDto> Faqs { get; set; } = new List<FaqItemDto>();
    }

    public class IndexPointDto
    {
        public DateTimeOffset Date { get; set; }
        public decimal IndexValue { get; set; }       // مقدار شاخص، نه قیمت
    }

    public class PlanFeatureDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class FaqItemDto
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}


using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlanCommand
{
    public sealed class BuyPlanResultDto
    {
        public decimal AmountRial { get; set; }
        public string NationalCode { get; set; }
        public string BirthDate { get; set; }

        public long PolicyId { get; set; }
        public long KarizmahOrderId { get; set; }


        public decimal GramPrice { get; set; }
        public decimal EstimatedGrams { get; set; }


        public InvestmentPlanType PlanType { get; set; }

        public decimal Grams { get; set; }              // مقدار طلای خریداری شده

        public decimal DailyChangePercent { get; set; } // همانی که در کارت سبز نشان می‌دهی


        public long OrderId { get; set; }               // شناسه سفارش کاریزما (اگر در پاسخ داری)
        public long TraceId { get; set; }               // TraceID یکتای سفارش
        public string ProviderStatus { get; set; }
    }

}

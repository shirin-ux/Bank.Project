using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlanCommand
{
    public sealed class BuyPlanResultDto
    {
        public decimal AmountRial { get; set; }
        public string NationalCode { get; set; }
        public string BirthDate { get; set; }

        public Guid ProviderPolicyId { get; set; }

        public decimal GramPrice { get; set; }
        public decimal EstimatedGrams { get; set; }


        public InvestmentPlanType PlanType { get; set; }

        public decimal Grams { get; set; }             

        public decimal DailyChangePercent { get; set; }


        public long OrderId { get; set; }            
        public long TraceId { get; set; }           
        public bool IsRepeated { get; set; }  
        public string ProviderStatus { get; set; }
    }

}

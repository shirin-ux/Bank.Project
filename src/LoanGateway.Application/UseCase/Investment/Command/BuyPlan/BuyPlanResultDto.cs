using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlan
{
    public sealed class BuyPlanResultDto
    {
        public decimal AmountRial { get; set; }
        public string NationalCode { get; set; }
        public string BirthDate { get; set; }
        public string PostalCode { get; set; }

        public Guid ProviderPolicyId { get; set; }

        public decimal GramPrice { get; set; }


        public InvestmentPlanType PlanType { get; set; }

        public decimal Grams { get; set; }             

        public decimal DailyChangePercent { get; set; }


        public Guid? OrderId { get; set; }            
        public long? TraceId { get; set; }           
        public bool? IsRepeated { get; set; }  
        public string ProviderStatus { get; set; }
    }

}

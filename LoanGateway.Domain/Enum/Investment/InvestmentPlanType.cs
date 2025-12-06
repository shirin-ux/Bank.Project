namespace LoanService.Domain.Enum.Investment;

public enum InvestmentPlanType
{

    Gold = 1,
    Silver = 2,
    FixedIncome = 3
}
public static class InvestmentPlanTypeExtensions
{
    public static string ToInstrumentId(this InvestmentPlanType plan)
    {
        return plan switch
        {
            InvestmentPlanType.Gold => "IRTICHGOLD01",
            InvestmentPlanType.Silver => "IRTICHSILV01",
 
            _ => throw new ArgumentOutOfRangeException(nameof(plan), plan, null)
        };
    }
}
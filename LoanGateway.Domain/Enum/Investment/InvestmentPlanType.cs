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
            InvestmentPlanType.Silver => "<<<PUT_SILVER_CODE_HERE>>>",
            InvestmentPlanType.FixedIncome => "<<<PUT_FIXEDINCOME_CODE_HERE>>>",
            _ => throw new ArgumentOutOfRangeException(nameof(plan), plan, null)
        };
    }
}
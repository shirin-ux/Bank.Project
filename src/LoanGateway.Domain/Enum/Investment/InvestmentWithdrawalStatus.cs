namespace LoanService.Domain.Enum.Investment
{
    public enum InvestmentWithdrawalStatus : byte
    {
        WaitingForOtp = 0,  
        Confirmed = 1,       
        Settled = 2,      
        Failed = 3,
        Cancelled = 4
    }
}

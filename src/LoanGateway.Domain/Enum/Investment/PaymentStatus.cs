namespace LoanService.Domain.Enum.Investment;

public enum PaymentStatus
{
    Created = 1,
    Redirected = 2,
    CallbackReceived = 3,
    Verified = 4,
    Failed = 5,
    Canceled = 6,
    TokenIssued = 7

}

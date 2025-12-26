namespace LoanGateway.Auth.Domain.Enum
{
    public enum VerificationStatus : byte
    {
        NotStarted = 0,
        Pending = 1,
        Verified = 2,
        Rejected = 3,
        Error = 4
    }
}

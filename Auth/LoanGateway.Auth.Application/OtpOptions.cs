namespace LoanGateway.Auth.Application
{

    public sealed class OtpOptions
    {
        public int CodeLength { get; set; } = 6;
        public int ExpiryMinutes { get; set; } =2;
        public int MaxAttempts { get; set; } = 5;
        public int MaxRequestsPerWindow { get; set; } = 3;
        public int RequestWindowMinutes { get; set; } = 5;
    }


}

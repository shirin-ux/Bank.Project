using LoanGateway.Auth.Domain.Enum;


namespace LoanGateway.Auth.Domain.Entities
{
    public class OtpCode:BaseEntity
    {
        public Guid? UserId { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public OtpPurpose Purpose { get; set; }
        public byte[] CodeHash { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? ConsumedAtUtc { get; set; }
        public int FailedAttempts { get; set; }
        public int MaxAttempts { get; set; }
        public string? RequestIp { get; set; }
        public string? UserAgent { get; set; }
        public bool? IsDeleted { get; set; }


        public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

        public bool IsConsumed => ConsumedAtUtc.HasValue;



        public void Consume(DateTime utcNow)
        {
            ConsumedAtUtc = utcNow;
        }

        public void RegisterFailedAttempt()
        {
            FailedAttempts++;
        }
    }
}

using LoanGateway.Auth.Domain.Aggregates;
using LoanGateway.Auth.Domain.Enum;

namespace LoanGateway.Auth.Domain.Entities;

public class User : AggregateRoot
{
    private User() { } // برای EF

    private User(Guid id, string mobile)
    {
        Id = id;
        Mobile = mobile;
        IsMobileVerified = false;
        IsKycCompleted = false;
        KycLevel = KycLevel.None;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public string Mobile { get; private set; } = default!;
    public string? NationalCode { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Email { get; private set; }
    public bool IsMobileVerified { get; private set; }
    public bool IsKycCompleted { get; private set; }
    public bool IsActive { get; private set; }
    public KycLevel KycLevel { get; private set; }
    public string? UidUserId { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public KycVerification? KycVerification { get; private set; }
    public static User CreateNew(string mobile)
      => new User(Guid.NewGuid(), mobile);

    public void SetNationalCode(string nationalCode)
    {
        NationalCode = nationalCode;
        Touch();
    }

    public void SetName(string? firstName, string? lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        Touch();
    }


    public void SetEmail(string? email)
    {
        Email = email;
        Touch();
    }

    public void SetKycCompleted(KycLevel level, string uidUserId)
    {
        IsKycCompleted = true;
        KycLevel = level;
        UidUserId = uidUserId;
        Touch();
    }
    public void MarkLogin()
    {
        LastLoginAtUtc = DateTime.UtcNow;
        Touch();
    }
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

}
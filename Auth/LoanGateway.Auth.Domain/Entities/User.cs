using LoanGateway.Auth.Domain.Aggregates;
using LoanGateway.Auth.Domain.Enum;

namespace LoanGateway.Auth.Domain.Entities;

public class User : AggregateRoot
{
    private User() { } // برای EF

    private User(Guid id, string mobile)
    {
        Id = id;

        MobileNumber = mobile;
        IsMobileVerified = false;
        IsProfileCompleted = false;
        IsKycCompleted = false;
        KycLevel = KycLevel.None;
        // کاربر تازه‌ثبت‌نام‌شده تا قبل از تکمیل پروفایل، غیرفعال در نظر گرفته می‌شود
        IsActive = false;
        CreatedAtUtc = DateTime.UtcNow;
    }
    public string MobileNumber { get; set; } = default!;
    public string? NationalCode { get; set; }
    public string? PostalCode { get; set; }
    public string? BirthDate { get; set; }
    public string? FirstName { get; set; }
    public string? ShenasnamehNumber { get; set; }
    public string? ShenasnameSerial { get; set; }
    public string? ShenasnameSeri { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public bool IsMobileVerified { get; set; }
    public bool IsKycCompleted { get; private set; }
    public bool IsProfileCompleted { get; set; }
    public bool IsActive { get;  set; }
    public KycLevel KycLevel { get; private set; }
    public string? UidUserId { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public bool IsKycVerified { get; private set; }
  
    public KycVerification? KycVerification { get; private set; }
    public bool? GiftStatus { get; set; } = false; // true = واجد کارت هدیه
    public object? IsDeleted { get; set; }

    public static User CreateNew(string mobile)
        => new User(Guid.NewGuid(), mobile);
    public void CompleteProfileAfterKyc(
        string nationalCode,
        string firstName,
        string lastName,
        string birthdate,
        string shenasnamehNumber,
        string shenasnameSerial,
        string shenasnameSeri
    /*    string postalCode*/,
        DateTime nowUtc)
    {
        NationalCode = nationalCode;
        FirstName = firstName;
        BirthDate = birthdate;
        LastName = lastName;
        IsProfileCompleted = true;
        IsKycVerified = true;
        ShenasnameSerial = shenasnameSerial;
        ShenasnameSeri = shenasnameSeri;
        ShenasnamehNumber = shenasnamehNumber;
        // پس از تکمیل احراز هویت و پروفایل، حساب فعال می‌شود
        IsActive = true;
        UpdatedAtUtc = nowUtc;
        //PostalCode = postalCode;
    }
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
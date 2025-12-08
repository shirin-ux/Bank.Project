using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.Enum;


namespace LoanGateway.Auth.Domain.Aggregates;
//دفترچه احراز هویت کاربر مرحله به مرحله 
public class KycVerification : BaseEntity
{
    private KycVerification(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;

        ShahkarStatus = VerificationStatus.NotStarted;
        PersonalInfoStatus = VerificationStatus.NotStarted;
        BankAccountStatus = VerificationStatus.NotStarted;
        AddressStatus = VerificationStatus.NotStarted;
        EkycStatus = VerificationStatus.NotStarted;

        CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;

    public VerificationStatus ShahkarStatus { get; private set; }
    public string? ShahkarMessage { get; private set; }

    public VerificationStatus PersonalInfoStatus { get; private set; }
    public string? PersonalInfoMessage { get; private set; }


    public VerificationStatus BankAccountStatus { get; private set; }
    public string? BankAccountMessage { get; private set; }


    public VerificationStatus AddressStatus { get; private set; }
    public string? AddressMessage { get; private set; }

    public VerificationStatus EkycStatus { get; private set; }
    public string? EkycMessage { get; private set; }


    public DateTime? LastShahkarAtUtc { get; private set; }
    public DateTime? LastPersonalInfoAtUtc { get; private set; }
    public DateTime? LastBankAccountAtUtc { get; private set; }
    public DateTime? LastAddressAtUtc { get; private set; }
    public DateTime? LastEkycAtUtc { get; private set; }
    /// <summary>
    ///  Provider (برای دیباگ / آدیت)
    /// </summary>
    public string? LastProviderPayloadJson { get; private set; }


    public static KycVerification CreateForUser(Guid userId)
        => new KycVerification(Guid.NewGuid(), userId);

    public void SetShahkarResult(VerificationStatus status, string? message, string? rawPayloadJson = null)
    {
        ShahkarStatus = status;
        ShahkarMessage = message;
        LastShahkarAtUtc = DateTime.UtcNow;
        UpdatePayload(rawPayloadJson);
    }

    public void SetPersonalInfoResult(VerificationStatus status, string? message, string? rawPayloadJson = null)
    {
        PersonalInfoStatus = status;
        PersonalInfoMessage = message;
        LastPersonalInfoAtUtc = DateTime.UtcNow;
        UpdatePayload(rawPayloadJson);
    }


    public void SetBankAccountResult(VerificationStatus status, string? message, string? rawPayloadJson = null)
    {
        BankAccountStatus = status;
        BankAccountMessage = message;
        LastBankAccountAtUtc = DateTime.UtcNow;
        UpdatePayload(rawPayloadJson);
    }

    public void SetAddressResult(VerificationStatus status, string? message, string? rawPayloadJson = null)
    {
        AddressStatus = status;
        AddressMessage = message;
        LastAddressAtUtc = DateTime.UtcNow;
        UpdatePayload(rawPayloadJson);
    }


    public void SetEkycResult(VerificationStatus status, string? message, string? rawPayloadJson = null)
    {
        EkycStatus = status;
        EkycMessage = message;
        LastEkycAtUtc = DateTime.UtcNow;
        UpdatePayload(rawPayloadJson);
    }

    private void UpdatePayload(string? payload)
    {
        if (!string.IsNullOrWhiteSpace(payload))
            LastProviderPayloadJson = payload;

        Touch();
    }
}

using LoanService.Domain.Enum;

namespace LoanService.Domain.Entities;

public sealed class ContractInfo
{
    /// <summary>
    /// اطلاعات قراردادها و فایل‌های امضا شده
    /// </summary>
    /// <param name="ApprovalCode"></param>
    /// <param name="WithCollateral"></param>
    /// <param name="ContractNumber"></param>
    /// <param name="Desc"></param>
    /// <param name="SignedContractBase64"></param>
    public Guid Id { get; set; }
    public Guid LoanRequestId { get; set; }
    public decimal? ApprovalCode { get; set; }
    public decimal ContractNumber { get; set; }


    public string? NationalCode { get; set; }


    public DateTime? BirthDate { get; set; }


    public string? MobileNumber { get; set; } = null!;

    public string? PostalCode { get; set; } = null!;


    public string? PhoneNumber { get; set; }

    public decimal? LoanAmount { get; set; }


    public short? InstallmentCount { get; set; }


    public CollateralType? CollateralType { get; set; } = null!;

    public decimal? CollateralNo { get; set; }


    public string? CollateralDate { get; set; } = null!;


    public decimal? CollateralAmount { get; set; }


    public string?   GuarantorNC { get; set; } = null!;


    public string? CollateralIssuer { get; set; }
    public string? ContractPath { get; set; }


    public string? ChequeSerial { get; set; }


    public string? Address { get; set; }
    public decimal? cbTrackingCode { get; set; }
}
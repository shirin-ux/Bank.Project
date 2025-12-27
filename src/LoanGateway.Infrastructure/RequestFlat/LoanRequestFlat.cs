using LoanService.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Infrastructure.RequestFlat;

public class LoanRequestFlat
{
    public Guid Id { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public LoanRequestState State { get; set; }
    public bool RequiresOtp { get; set; }

    // UserId
    public Guid UserId { get; set; }

    // ProviderInfo (ValueObject)
    public int Provider_Type { get; set; }
    public decimal? Provider_ApprovalCode { get; set; }
    public bool Provider_RequiresOtp { get; set; }

    // InqueryRequest (ValueObject)
    public string? InquiryRequest_Id { get; set; }

    // PayRequest
    public string? PayRequest_Id { get; set; }
    public decimal? PayRequest_RequestedAmount { get; set; }

    // DecisionStamp
    public int? Decision_ErrorCode { get; set; }
    public string? Decision_ErrorMessage { get; set; }
    public int? Decision_ReasonCode { get; set; }
    public string? Decision_ReasonMessage { get; set; }

    // GrantRequest
    public decimal? Grant_ContractId { get; set; }
    public string? Grant_Status { get; set; }
    public string? Grant_SignedContractBase64 { get; set; }
    public string? Grant_RequestedAmount { get; set; }

    public int? LastReasonCode { get; set; }
    public string? LastReasonMessage { get; set; }
    public int? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public int RetryCount { get; set; }
}


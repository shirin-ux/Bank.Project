

using LoanService.Domain.Entities;

namespace Common;
public record ProviderDecisionResult(

     bool? IsSuccess,
    LoanRequestState? NextState,
    string UiMessage,
    int ReasonCode,
    bool? Retryable 
);

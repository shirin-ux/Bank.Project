namespace LoanService.Domain.Entities.Investment
{
    public sealed record VerifyPayment(
        int VerifyResCode,
        int VerifiedAmountRials,
        string? RetrievalRefNo,
        string? SystemTraceNo,
         long OrderId
        );

}

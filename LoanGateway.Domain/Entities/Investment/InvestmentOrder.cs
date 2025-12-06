using LoanService.Domain.Enum.Investment;

namespace LoanService.Domain.Entities.Investment;

public class InvestmentOrder : BaseEntity
{
    private InvestmentOrder() { }

    private InvestmentOrder(
        Guid id,
        Guid userId,
        InvestmentPlanType planType,
        long amount,
        decimal? requestedGram,
        long karizmahOrderId,
        long traceId,
        string paymentUrl)
    {
        Id = id;
        UserId = userId;
        PlanType = planType;
        Amount = amount;
        RequestedGram = requestedGram;
        KarizmahOrderId = karizmahOrderId;
        TraceId = traceId;
        PaymentUrl = paymentUrl;

        State = InvestmentOrderState.PendingPayment;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public InvestmentPlanType PlanType { get; private set; }


    public long Amount { get; private set; }


    public decimal? RequestedGram { get; private set; }


    public long KarizmahOrderId { get; private set; }


    public long TraceId { get; private set; }

    public string PaymentUrl { get; private set; }

    public InvestmentOrderState State { get; private set; }

    public string? BankReferenceId { get; private set; }
    public long? PolicyId { get; private set; }

    public static InvestmentOrder Create(
        Guid userId,
        InvestmentPlanType planType,
        long amount,
        decimal? requestedGram,
        long karizmahOrderId,
        long traceId,
        string paymentUrl)
    {
        return new InvestmentOrder(
            Guid.NewGuid(),
            userId,
            planType,
            amount,
            requestedGram,
            karizmahOrderId,
            traceId,
            paymentUrl);
    }

    public void MarkPaid(string bankRef, long? policyId)
    {
        State = InvestmentOrderState.Paid;
        BankReferenceId = bankRef;
        PolicyId = policyId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string? bankRef = null)
    {
        State = InvestmentOrderState.Failed;
        if (!string.IsNullOrWhiteSpace(bankRef))
            BankReferenceId = bankRef;

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        State = InvestmentOrderState.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}



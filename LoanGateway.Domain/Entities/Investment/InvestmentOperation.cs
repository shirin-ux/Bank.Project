using LoanService.Domain.Enum.Investment;

namespace LoanService.Domain.Entities.Investment
{
    public sealed class InvestmentOperation : BaseEntity
    {
        private InvestmentOperation() { }

        public long? PolicyId { get; private set; }
        public string? TraceId { get; private set; } = default!;
        public InvestmentOperationType Type { get; private set; }
        public InvestmentOrderState Status { get; private set; }

        public decimal Amount { get; private set; }
        public DateTime OperationDate { get; private set; }
        public DateTime? ReceiptDate { get; private set; }
        public string? ReceiptNumber { get; private set; }
        public string? Description { get; private set; }

        public static InvestmentOperation CreateIncreaseDirect(long? policyId, decimal amount,string traceId,DateTime receiptDate,string receiptNumber, string? description)
        {
            return new InvestmentOperation
            {
                Id = Guid.NewGuid(),
                PolicyId = policyId,
                Amount = amount,
                TraceId = traceId,
                Type = InvestmentOperationType.IncreaseDirect,
                Status = InvestmentOrderState.Created,
                OperationDate = DateTime.UtcNow,
                ReceiptDate = receiptDate,
                ReceiptNumber = receiptNumber,
                Description = description
            };
        }

        public static InvestmentOperation CreateIncreaseOnlineRequested( long? policyId, decimal amount,string traceId, string? description)
        {
            return new InvestmentOperation
            {
                Id = Guid.NewGuid(),
                PolicyId = policyId,
                Amount = amount,
                TraceId = traceId,
                Type = InvestmentOperationType.IncreaseOnline,
                Status = InvestmentOrderState.PendingPayment,
                OperationDate = DateTime.UtcNow,
                Description = description
            };
        }


        public static InvestmentOperation CreateDecreaseDirect(long? policyId,decimal amount,string traceId,DateTime receiptDate,string? description)
        {
            return new InvestmentOperation
            {
                Id = Guid.NewGuid(),
                PolicyId = policyId,
                Amount = amount,
                TraceId = traceId,
                Type = InvestmentOperationType.DecreaseDirect,
                Status = InvestmentOrderState.Created,
                OperationDate = DateTime.UtcNow,
                ReceiptDate = receiptDate,
                Description = description
            };
        }

        public static InvestmentOperation CreateAccount( long? policyId, string? traceId,string? description = null)
        {
            return new InvestmentOperation
            {
                Id = Guid.NewGuid(),
                PolicyId = policyId,
                Amount = 0,
                TraceId = traceId,
                Type = InvestmentOperationType.CreateAccount,
                Status = InvestmentOrderState.Created,
                OperationDate = DateTime.UtcNow,
                Description = description
            };
        }

        public void MarkCompleted(string? receiptNumber = null, DateTime? receiptDate = null)
        {
            Status = InvestmentOrderState.Created;
            ReceiptNumber ??= receiptNumber;
            ReceiptDate ??= receiptDate;
        }

        public void MarkFailed(string? description = null)
        {
            Status = InvestmentOrderState.Failed;
            if (!string.IsNullOrWhiteSpace(description))
                Description = description;
        }
    }
}

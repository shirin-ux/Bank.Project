using LoanService.Domain.Enum.Investment;

namespace LoanService.Domain.Entities.Investment
{
    public sealed class InvestmentWithdrawal : BaseEntity
    {
        public Guid InvestmentAccountId { get; private set; }
        public decimal Amount { get; private set; }
        public decimal Fee { get; private set; }
        public decimal FinalAmount => Amount - Fee;
        public string DestinationIban { get; private set; } = default!;
        public long ProviderOrderId { get; private set; }
        public long TraceId { get; private set; }
        public InvestmentWithdrawalStatus Status { get; private set; }

        private InvestmentWithdrawal() { }

        public static InvestmentWithdrawal Create(
            Guid accountId,
            decimal amount,
            decimal fee,
            string destinationIban,
            long providerOrderId,
            long traceId)
            => new()
            {
                Id = Guid.NewGuid(),
                InvestmentAccountId = accountId,
                Amount = amount,
                Fee = fee,
                DestinationIban = destinationIban,
                ProviderOrderId = providerOrderId,
                TraceId = traceId,
                Status = InvestmentWithdrawalStatus.WaitingForOtp,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

        public void MarkOtpConfirmed()
        {
            Status = InvestmentWithdrawalStatus.Confirmed;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void MarkFailed()
        {
            Status = InvestmentWithdrawalStatus.Failed;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}


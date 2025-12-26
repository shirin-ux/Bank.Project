

namespace LoanService.Application.UseCase.Investment.Command.InvestmentWithdrawal
{
   public class StartWithdrawalResultDto
    {
        public Guid WithdrawalId { get; init; }
        public long ProviderOrderId { get; init; }
        public long TraceId { get; init; }

        public decimal Amount { get; init; }
        public decimal Fee { get; init; }
        public decimal FinalPayableAmount { get; init; }

        public string DestinationIban { get; init; } = default!;
        public string MaskedPhoneNumber { get; init; } = default!;
        public string SettlementText { get; init; } = "۱ الی ۲ روز کاری";
    }
}

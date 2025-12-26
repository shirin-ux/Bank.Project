using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.TransferRegister;

public sealed record TransferRegisterCommand : IRequest<TransferRegisterResultDto>
{
    public decimal ApprovalCode { get; init; }

    public int TransferDate { get; init; }
    public decimal PayAmount { get; init; }
    public string DestIban { get; init; } = default!;
    public string DestNationalId { get; init; } = default!;
    public string DestName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public ProviderType ProviderType { get; set; }
    /// <summary>
    /// بسته به نوع مصوبه، referenceNo می‌تواند "شماره قرارداد" یا "شماره تراکنش" باشد.
    /// </summary>
    public IReadOnlyList<TransferDetailItem> Details { get; init; } = Array.Empty<TransferDetailItem>();

    /// <summary>
    /// اگر سمت بانک نیاز داشته باشد می‌توانی این را هم بفرستی؛ در خروجی قطعی است.
    /// 1: پایا, 2: ساتنا, 3: پل, 4: ملت به ملت
    /// </summary>
    public short? TransType { get; init; }

    public sealed record TransferDetailItem
    {
        public decimal ReferenceNo { get; init; }
        public decimal Amount { get; init; }
    }
}

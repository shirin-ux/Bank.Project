using Common;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.DepositRequest;
public sealed record DepositRequestCommand : IRequest<DepositRequestResultDto>
{
    public BankProviderType ProviderType { get; set; }
    public decimal ContractNumber { get; init; }
    public string BuyerNationalCode { get; init; } = default!;
    public string? SellerNationalCode { get; init; }
    public decimal? SellerAccountNo { get; init; }
    public decimal PayAmount { get; init; }
    public string? TransactionDesc { get; init; }

    public depositType? DepositType { get; init; }
    public int? OtpCode { get; init; }

}
public enum depositType : short
{
    /// <summary>
    /// واریز به هر حساب (به حساب فروشنده)
    /// sellerAccountNo و sellerNationalCode الزامی هستند.
    /// </summary>
    AnyAccount = 1,

    /// <summary>
    /// واریز به حساب ثابت (مشخص در مصوبه)
    /// در این حالت اطلاعات فروشنده از مصوبه خوانده می‌شود.
    /// </summary>
    FixedAccount = 2,

    /// <summary>
    /// واریز به حساب مشتری (خود وام‌گیرنده)
    /// برای تسویه مستقیم به حساب مشتری استفاده می‌شود.
    /// </summary>
    CustomerAccount = 3
}

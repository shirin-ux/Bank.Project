using FluentValidation;

namespace LoanService.Application.UseCase.Command.DepositRequest;
public sealed class DepositRequestValidator : AbstractValidator<DepositRequestCommand>
{
    public DepositRequestValidator()
    {
        RuleFor(x => x.ProviderType)
            .IsInEnum()
            .WithMessage("نوع بانک نامعتبر است.");

        RuleFor(x => x.ContractNumber)
            .GreaterThan(0)
            .NotNull()
            .WithMessage("شماره قرارداد الزامی است.");

        RuleFor(x => x.BuyerNationalCode)
            .NotEmpty().WithMessage("کد ملی خریدار اجباری است.")
            .Length(10).WithMessage("کد ملی خریدار باید ۱۰ رقم باشد.")
            .Matches(@"^\d{10}$").WithMessage("کد ملی خریدار باید فقط شامل ارقام باشد.");

        RuleFor(x => x.PayAmount)
            .GreaterThan(0)
            .NotNull()
            .WithMessage("مبلغ پرداخت باید بزرگ‌تر از صفر باشد.");

        RuleFor(x => x.TransactionDesc)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.TransactionDesc))
            .WithMessage("توضیحات تراکنش حداکثر می‌تواند ۵۰۰ کاراکتر باشد.");

        RuleFor(x => x.DepositType)

            .IsInEnum().When(x => x.DepositType.HasValue)
            .WithMessage("نوع واریز (DepositType) نامعتبر است.");


        When(x => x.DepositType == depositType.AnyAccount, () =>
        {
            RuleFor(x => x.SellerNationalCode)
                .Length(10).WithMessage("کد ملی باید ۱۰ رقم باشد.")
                .Matches(@"^\d{10}$").WithMessage("کد ملی باید فقط شامل ارقام باشد.");

            RuleFor(x => x.SellerAccountNo)
             .Matches(@"^\d{10,20}$")
              .WithMessage("شماره حساب نامعتبر است .");

            RuleFor(x => x.OtpCode)
                 .NotNull().WithMessage("ارسال  رمز یکبار مصرف اجباری است.");
        });


    }
}

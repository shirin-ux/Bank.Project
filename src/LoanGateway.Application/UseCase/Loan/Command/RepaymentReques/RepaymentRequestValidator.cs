using FluentValidation;


namespace LoanService.Application.UseCase.Loan.Command.RepaymentReques;

public sealed class RepaymentRequestValidator : AbstractValidator<RepaymentRequestCommand>
{
    public RepaymentRequestValidator()
    {
        RuleFor(x => x.AccountNo)
                     .NotEmpty().WithMessage("شماره حساب اجباری است.")
                     .MaximumLength(16).WithMessage("شماره حساب حداکثر می‌تواند 16 کاراکتر باشد.")
                     .Matches(@"^\d+$").WithMessage("شماره حساب باید فقط شامل ارقام باشد.");

        RuleFor(x => x.ContractNo)
                     .MaximumLength(16).WithMessage("شماره قرار داد حداکثر می‌تواند 16 کاراکتر باشد.")
                     .Matches(@"^\d+$").WithMessage("شماره قرار داد باید فقط شامل ارقام باشد.");

        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کد ملی اجباری است.")
            .Length(10).WithMessage("کد ملی باید ۱۰ رقم باشد.")
            .Matches(@"^\d{10}$").WithMessage("کد ملی نامعتبر است.");

        RuleFor(x => x.RepaymentAmount)
            .GreaterThan(0)
            .WithMessage("مبلغ بازپرداخت نامعتبر است.");



        RuleFor(x => x.OtpCode)
            .GreaterThan(0)
            .When(x => x.OtpCode.HasValue)
            .WithMessage("رمز یکبار مصرف نامعتبر است");
    }
}

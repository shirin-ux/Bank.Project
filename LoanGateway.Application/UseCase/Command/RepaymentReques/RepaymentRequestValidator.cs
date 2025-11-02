using FluentValidation;


namespace LoanService.Application.UseCase.Command.RepaymentReques;

public sealed class RepaymentRequestValidator : AbstractValidator<RepaymentRequestCommand>
{
    public RepaymentRequestValidator()
    {
        //RuleFor(x => x.AccountNo);
        //RuleFor(x => x.ContractNo).GreaterThan(0);
        RuleFor(x => x.NationalCode).NotEmpty().Matches(@"^\d{10}$");
        RuleFor(x => x.RepaymentAmount).GreaterThan(0);
        // اگر سیاست مصوبه ایجاب کند:
        // RuleFor(x => x.OtpCode).NotNull();
    }
}

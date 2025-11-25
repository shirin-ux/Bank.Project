using FluentValidation;
using static LoanService.Application.UseCase.Loan.Command.OtpRequest.OtpRequestCommand;


namespace LoanService.Application.UseCase.Loan.Command.OtpRequest
{
    public sealed class OtpRequestValidator : AbstractValidator<OtpRequestCommand>
    {
        public OtpRequestValidator()
        {
            RuleFor(x => x.ContractNumber)
            .GreaterThan(0)
            .WithMessage("شماره قرارداد باید بزرگ‌تر از صفر باشد.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("شماره حساب اجباری است.")
                .MaximumLength(16).WithMessage("شماره حساب حداکثر می‌تواند 16 کاراکتر باشد.")
                .Matches(@"^\d+$").WithMessage("شماره حساب باید فقط شامل ارقام باشد.");

            RuleFor(x => x.NationalCode)
                .NotEmpty().WithMessage("کد ملی اجباری است.")
                .Length(10).WithMessage("کد ملی باید ۱۰ رقم باشد.")
                .Matches(@"^\d{10}$").WithMessage("کد ملی باید فقط شامل ارقام باشد.");

            RuleFor(x => x.PayAmount)
                .GreaterThan(0)
                .WithMessage("مبلغ پرداخت باید بزرگ‌تر از صفر باشد.");

            RuleFor(x => x.ProviderType)
                .IsInEnum()
                .WithMessage("نوع بانک نامعتبر است.");

            RuleFor(x => x.ServiceType)
                .IsInEnum()
                .WithMessage("نوع سرویس نامعتبر است.");

            When(x => x.ServiceType == serviceType.repayment, () =>
            {
                RuleFor(x => x.AccountNumber)
                        .NotNull()
                        .WithMessage("شماره حساب الزامی است");

            });

        }
    }

}

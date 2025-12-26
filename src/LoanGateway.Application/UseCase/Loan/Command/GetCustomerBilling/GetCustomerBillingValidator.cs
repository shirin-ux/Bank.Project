using FluentValidation;

namespace LoanService.Application.UseCase.Loan.Command.GetCustomerBilling;

public sealed class GetCustomerCreditBalanceValidator : AbstractValidator<GetCustomerBillingCommand>
{
    public GetCustomerCreditBalanceValidator()
    {
        RuleFor(x => x.BillingNumber)
                 .GreaterThanOrEqualTo((short)0)
                 .WithMessage("شماره صورتحساب نمی‌تواند منفی باشد.");

        RuleFor(x => x.ContractNumber)
                      .MaximumLength(16).WithMessage("شماره قرار داد حداکثر می‌تواند 16 کاراکتر باشد.")
                     .Matches(@"^\d+$").WithMessage("شماره قرار داد باید فقط شامل ارقام باشد.");

        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کد ملی اجباری است.")
            .Length(10).WithMessage("کد ملی باید ۱۰ رقم باشد.")
            .Matches(@"^\d{10}$").WithMessage("کد ملی باید فقط شامل ارقام باشد.");

    }
}

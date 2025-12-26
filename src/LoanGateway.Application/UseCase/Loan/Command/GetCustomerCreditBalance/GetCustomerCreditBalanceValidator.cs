using FluentValidation;
using System.Text.RegularExpressions;

namespace LoanService.Application.UseCase.Loan.Command.GetCustomerCreditBalance;

public sealed class GetCustomerCreditBalanceValidator : AbstractValidator<GetCustomerCreditBalanceCommand>
{
    public GetCustomerCreditBalanceValidator()
    {
        RuleFor(x => x.NationalCode)
        .NotEmpty().WithMessage("کد ملی الزامی است.")
        .Length(10).WithMessage("کد ملی باید ۱۰ رقم باشد.")
        .Matches(@"^\d{10}$").WithMessage("کد ملی فقط باید شامل عدد باشد.")
        .Must(BeValidIranianNationalCode).WithMessage("کد ملی نامعتبر است.");

        RuleFor(x => x.ContractNumber)
                      .MaximumLength(16).WithMessage("شماره قرار داد حداکثر می‌تواند 16 کاراکتر باشد.")
                     .Matches(@"^\d+$").WithMessage("شماره قرار داد باید فقط شامل ارقام باشد.");

    }
    private bool BeValidIranianNationalCode(string nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10)
            return false;

        if (!Regex.IsMatch(nationalCode, @"^\d{10}$"))
            return false;


        var invalids = new[]
        {
            "0000000000","1111111111","2222222222","3333333333","4444444444",
            "5555555555","6666666666","7777777777","8888888888","9999999999"
        };
        if (invalids.Contains(nationalCode))
            return false;


        var check = nationalCode[9] - '0';
        var sum = 0;

        for (int i = 0; i < 9; i++)
            sum += (nationalCode[i] - '0') * (10 - i);

        var remainder = sum % 11;

        return (remainder < 2 && check == remainder) ||
               (remainder >= 2 && check == 11 - remainder);
    }

}

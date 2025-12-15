using FluentValidation;

namespace LoanService.Application.UseCase.Investment.Command.User
{
    public class ZipCodeRequestValidator : AbstractValidator<ReceiveZipCodeCommand>
    {
        public ZipCodeRequestValidator()
        {
            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("کدپستی الزامی است.")
                .Length(10).WithMessage("کدپستی باید دقیقاً ۱۰ رقم باشد.")
                .Matches(@"^\d{10}$").WithMessage("کدپستی باید فقط شامل اعداد باشد.")
                .Must(BeValidIranianZipCode).WithMessage("کدپستی معتبر نیست.");
        }

        private bool BeValidIranianZipCode(string zip)
        {

            if (string.IsNullOrEmpty(zip)) return false;
            return !new string(zip[0], zip.Length).Equals(zip); // همه اعداد یکسان نباشند
        }
    }
}

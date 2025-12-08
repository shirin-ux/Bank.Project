using FluentValidation;
namespace LoanGateway.Auth.Application.UseCase.Command.RegisterStart;

public sealed class RegisterStartCommandValidator
    : AbstractValidator<RegisterStartCommand>
{
    public RegisterStartCommandValidator()
    {
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Length(11).WithMessage("شماره موبایل باید 11 رقم باشد.")
            .Matches("^09[0-9]{9}$").WithMessage("فرمت شماره موبایل نامعتبر است.");

        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کدملی الزامی است.")
            .Length(10).WithMessage("کدملی باید 10 رقم باشد.")
            .Matches("^[0-9]{10}$").WithMessage("فرمت کدملی نامعتبر است.");
    }
}

namespace LoanGateway.Auth.Application.UseCase.Command.MobileOwner;
using FluentValidation;



public sealed class VerifyMobileOwnerCommandValidator
    : AbstractValidator<VerifyMobileOwnerCommand>
{
    public VerifyMobileOwnerCommandValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("کدملی الزامی است.")
            .Length(10).WithMessage("کدملی باید 10 رقم باشد.")
            .Matches("^[0-9]{10}$").WithMessage("کدملی فقط باید شامل عدد باشد.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Length(11).WithMessage("شماره موبایل باید 11 رقم باشد.")
            .Matches("^09[0-9]{9}$").WithMessage("شماره موبایل باید با 09 شروع شده و فقط عدد باشد.");
    }
}

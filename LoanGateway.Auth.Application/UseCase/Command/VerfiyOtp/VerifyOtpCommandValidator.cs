using FluentValidation;

namespace LoanGateway.Auth.Application.UseCase.Command.VerfiyOtp;
public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommandDto>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.MobileNumber)
                   .NotEmpty().WithMessage("شماره موبایل الزامی است.")
                   .MaximumLength(20)
                   .Matches(@"^09\d{9}$").WithMessage("فرمت شماره موبایل نامعتبر است.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد تأیید الزامی است.")
            .Length(4, 8).WithMessage("طول کد تأیید نامعتبر است.")
            .Matches(@"^\d+$").WithMessage("کد تأیید باید عددی باشد.");


        RuleFor(x => x.Purpose)
            .IsInEnum();
    }
}




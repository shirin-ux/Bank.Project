using FluentValidation;

namespace LoanGateway.Auth.Application.UseCase.Command.VerfiyOtp;
public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommandDto>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(4, 8) // بسته به طول کد
            .Matches(@"^\d+$")
            .WithMessage("کد باید فقط عدد باشد.");

        RuleFor(x => x.Purpose)
            .IsInEnum();
    }
}




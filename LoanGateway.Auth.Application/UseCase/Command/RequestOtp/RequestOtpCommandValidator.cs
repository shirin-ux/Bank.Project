using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.RequestOtp
{
    public sealed class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommandDto>
    {
        public RequestOtpCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20)
                .Matches(@"^09\d{9}$") 
                .WithMessage("شماره موبایل نامعتبر است.");

            RuleFor(x => x.Purpose)
                .IsInEnum();
        }
    }
}

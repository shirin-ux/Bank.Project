using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.RefreshToken
{


    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommandDto>
    {
        public RefreshTokenCommandValidator()
        {
            // AccessToken اختیاری است؛ فقط اگر ارسال شد، طول آن را چک می‌کنیم
            RuleFor(x => x.AccessToken)
                .MaximumLength(2000).WithMessage("Access token نامعتبر است.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token الزامی است.")
                .MaximumLength(500).WithMessage("Refresh token نامعتبر است.")
                .Must(token => !string.IsNullOrWhiteSpace(token) && token.Length >= 20)
                .WithMessage("فرمت refresh token نامعتبر است.");
        }
    }
}

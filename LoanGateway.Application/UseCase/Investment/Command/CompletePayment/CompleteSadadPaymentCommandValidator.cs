using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.CompletePayment;

public sealed class CompleteSadadPaymentCommandValidator : AbstractValidator<CompleteSadadPaymentCommand>
{
    public CompleteSadadPaymentCommandValidator()
    {
        RuleFor(x => x.Callback.OrderId);
        RuleFor(x => x.Callback.Token).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Callback.ResCode).InclusiveBetween(-99999, 99999);
    }
}
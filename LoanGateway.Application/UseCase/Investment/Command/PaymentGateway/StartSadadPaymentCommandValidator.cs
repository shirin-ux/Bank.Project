using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.PaymentGateway;

public sealed class StartSadadPaymentCommandValidator : FluentValidation.AbstractValidator<StartSadadPaymentCommand>
{
    public StartSadadPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId);
        RuleFor(x => x.AmountRials).GreaterThan(0);
    }
}
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerBilling;

public sealed class GetCustomerBillingValidator : AbstractValidator<GetCustomerBillingCommand>
{
    public GetCustomerBillingValidator()
    {
        RuleFor(x => x.BillingNumber).GreaterThanOrEqualTo((short)0);
        RuleFor(x => x.ContractNumber).GreaterThan(0);
        RuleFor(x => x.NationalCode).NotEmpty().Matches(@"^\d{10}$");
    }
}

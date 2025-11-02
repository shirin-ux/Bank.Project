using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;
public sealed class GetCustomerPurchaseDetailsValidator : AbstractValidator<GetCustomerPurchaseDetailsCommand>
{
    public GetCustomerPurchaseDetailsValidator()
    {
        RuleFor(x => x.ContractNumber).GreaterThan(0);
        RuleFor(x => x.NationalCode).NotEmpty().Matches(@"^\d{10}$");
        RuleFor(x => x.FromDate).NotEmpty().Matches(@"^\d{2}/\d{2}/\d{4}$");
        RuleFor(x => x.ToDate).NotEmpty().Matches(@"^\d{2}/\d{2}/\d{4}$");
    }
}
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.OtpRequest
{
    public sealed class OtpRequestValidator : AbstractValidator<OtpRequestCommand>
    {
        public OtpRequestValidator()
        {
            RuleFor(x => x.ContractNumber).GreaterThan(0);
            RuleFor(x => x.NationalCode).NotEmpty().Matches(@"^\d{10}$");
            //RuleFor(x => x.am).GreaterThan(0);
            //RuleFor(x => x.ServiceType).Must(s => s == 1 || s == 2)
            //    .WithMessage("serviceType must be 1 (pay) or 2 (repay).");

            //When(x => x.ServiceType == 2, () =>
            //{
            //    RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(20);
            //});
        }
    }
}

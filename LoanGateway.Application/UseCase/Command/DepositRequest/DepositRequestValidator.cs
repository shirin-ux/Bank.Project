using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.DepositRequest;
public sealed class DepositRequestValidator : AbstractValidator<DepositRequestCommand>
{
    public DepositRequestValidator()
    {
        RuleFor(x => x.ContractNumber).GreaterThan(0);
        RuleFor(x => x.BuyerNationalCode).NotEmpty().Matches(@"^\d{10}$");
        RuleFor(x => x.PayAmount).GreaterThan(0);

      
        //When(x => x.DepositType.HasValue, () =>
        //{
        //    RuleFor(x => x.DepositType!.Value).Must(v => v is 1 or 2 or 3)
        //        .WithMessage("depositType must be 1, 2, or 3.");
        //});

 
        //When(x => x.DepositType == 1, () =>
        //{
        //    RuleFor(x => x.SellerNationalCode).NotEmpty().Matches(@"^\d{10}$");
        //    RuleFor(x => x.SellerAccountNo).NotNull().GreaterThan(0);
        //});

       
        // RuleFor(x => x.OtpCode).NotNull();
    }
}



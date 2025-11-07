using FluentValidation;

namespace LoanService.Application.UseCase.Command.GetCollateralContractFile;
public class GetCollateralContractFileValidator : AbstractValidator<GetCollateralContractFileCommand>
{
    public GetCollateralContractFileValidator()
    {
        //RuleFor(x => x.LoanRequestId).NotEmpty();
        RuleFor(x => x.NationalCode)
            .Length(10)
            .Matches("^[0-9]+$").WithMessage("کدملی باید عددی باشد.");

        RuleFor(x => x.MobileNumber)
            .Matches("^09\\d{9}$").WithMessage("شماره موبایل معتبر نیست.");

        RuleFor(x => x.PostalCode)
            .Matches("^\\d{10}$").WithMessage("کدپستی باید 10 رقمی باشد.");

        //RuleFor(x => x.InstallmentCount)
        //    .GreaterThan(0);

        RuleFor(x => x.CollateralType).NotEmpty();
    }
}



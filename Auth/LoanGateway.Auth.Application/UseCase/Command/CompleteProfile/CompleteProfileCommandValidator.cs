using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.CompleteProfile;
public sealed class UserInfoCommandValidator : AbstractValidator<CompleteProfileCommand>
{
    public UserInfoCommandValidator()
    {
        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کدملی الزامی است.")
            .Length(10).WithMessage("طول کدملی باید 10 رقم باشد.")
            .Matches(@"^\d{10}$").WithMessage("کدملی باید فقط عدد باشد.");

        //RuleFor(x => x.BirthDate)
        //    .LessThan(DateTime.Today).WithMessage("تاریخ تولد نامعتبر میباشد.");

        //RuleFor(x => x.FirstName)
        //    .NotEmpty().WithMessage("نام الزامی است.")
        //    .MaximumLength(50);

        //RuleFor(x => x.LastName)
        //    .NotEmpty().WithMessage("نام خانوادگی الزامی است.")
        //    .MaximumLength(50);
    }
}

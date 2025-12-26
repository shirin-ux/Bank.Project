using FluentValidation;

namespace LoanGateway.Auth.Application.UseCase.Command.UserInfo;
public sealed class UserInfoCommandValidator : AbstractValidator<UserInfoCommand>
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

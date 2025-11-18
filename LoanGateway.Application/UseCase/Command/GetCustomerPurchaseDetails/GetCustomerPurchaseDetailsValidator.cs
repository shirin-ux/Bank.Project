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
        RuleFor(x => x.ContractNumber)
                   .MaximumLength(16).WithMessage("شماره قرارداد حداکثر می‌تواند 16 کاراکتر باشد.")
                   .Matches(@"^\d+$").WithMessage("شماره قرارداد باید فقط شامل ارقام باشد.");

        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کد ملی اجباری است.")
            .Matches(@"^\d{10}$").WithMessage("کد ملی نامعتبر است. کد ملی باید 10 رقم باشد.");

        RuleFor(x => x.FromDate)
            .NotEmpty().WithMessage("تاریخ شروع اجباری است.")
            .Matches(@"^\d{2}/\d{2}/\d{4}$")
            .WithMessage("فرمت تاریخ شروع نامعتبر است. قالب صحیح: dd/MM/yyyy");

        RuleFor(x => x.ToDate)
            .NotEmpty().WithMessage("تاریخ پایان اجباری است.")
            .Matches(@"^\d{2}/\d{2}/\d{4}$")
            .WithMessage("فرمت تاریخ پایان نامعتبر است. قالب صحیح: dd/MM/yyyy");
    }
}
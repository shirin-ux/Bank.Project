using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.CustomerInquiryStatus;

public class GetInquiryResultQueryValidator : AbstractValidator<GetCustomerInquiryStatusQuery>
{
    public GetInquiryResultQueryValidator()
    {
        RuleFor(x => x.LoanId)
            .NotEmpty()
            .WithMessage("شناسه درخواست وام (LoanId) الزامی است.");

        RuleFor(x => x.ProviderType)
            .IsInEnum()
            .WithMessage("نوع تأمین‌کننده مالی (ProviderType) نامعتبر است.");
    }
}
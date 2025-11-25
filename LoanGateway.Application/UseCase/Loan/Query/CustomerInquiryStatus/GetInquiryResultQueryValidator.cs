using FluentValidation;

namespace LoanService.Application.UseCase.Loan.Query.CustomerInquiryStatus;

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
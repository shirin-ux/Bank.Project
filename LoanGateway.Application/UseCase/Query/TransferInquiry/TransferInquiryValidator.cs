using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.TransferInquiry;

public sealed class TransferInquiryValidator : AbstractValidator<TransferInquiryQuery>
{
    public TransferInquiryValidator()
    {
        RuleFor(x => x.RegisterCode)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[A-Za-z0-9\-]+$"); 
    }
}
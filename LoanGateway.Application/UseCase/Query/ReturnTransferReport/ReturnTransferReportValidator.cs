using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.ReturnTransferReport
{
    public sealed class ReturnTransferReportValidator : AbstractValidator<ReturnTransferReportCommand>
    {
        public ReturnTransferReportValidator()
        {
            RuleFor(x => x.ReturnDate)
                .InclusiveBetween(19000101, 29991231); 
            RuleFor(x => x.FromId)
                .GreaterThanOrEqualTo(0); 
        }
    }
}

using FluentValidation;

namespace LoanService.Application.UseCase.Loan.Query.ReturnTransferReport
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

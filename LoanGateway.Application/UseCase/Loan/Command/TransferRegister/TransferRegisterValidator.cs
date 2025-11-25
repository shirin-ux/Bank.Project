using FluentValidation;

namespace LoanService.Application.UseCase.Loan.Command.TransferRegister;

public sealed class TransferRegisterValidator : AbstractValidator<TransferRegisterCommand>
{
    public TransferRegisterValidator()
    {
        RuleFor(x => x.ApprovalCode).GreaterThan(0);
        RuleFor(x => x.TransferDate)
            .InclusiveBetween(19000101, 29991231);

        RuleFor(x => x.PayAmount).GreaterThan(0);

        RuleFor(x => x.DestIban)
            .NotEmpty().MaximumLength(26)
            .Matches(@"^[A-Za-z0-9]{1,26}$");

        RuleFor(x => x.DestNationalId)
            .NotEmpty().Matches(@"^\d{1,15}$");

        RuleFor(x => x.DestName).NotEmpty().MaximumLength(70);

        RuleFor(x => x.Description).NotEmpty().MaximumLength(30);

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("details must contain at least one item.");

        RuleForEach(x => x.Details).ChildRules(d =>
        {
            d.RuleFor(y => y.ReferenceNo).GreaterThan(0);
            d.RuleFor(y => y.Amount).GreaterThan(0);
        });

        When(x => x.TransType.HasValue, () =>
        {
            RuleFor(x => x.TransType!.Value).Must(v => v is 1 or 2 or 3 or 4)
                .WithMessage("transType must be 1 (PAyA), 2 (SATNA), 3 (PEL), or 4 (Mellat2Mellat).");
        });
    }
}

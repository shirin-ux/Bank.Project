using FluentValidation;

namespace LoanService.Application.UseCase.Command.SubmitPayRequest;


public sealed class SubmitPayRequestCommandValidator
    : AbstractValidator<SubmitPayRequestCommand>
{
    public SubmitPayRequestCommandValidator()
    {
        RuleFor(x => x.ContractNumber)
            .NotEmpty().WithMessage("شماره قرارداد اجباری است.")
            .GreaterThan(0).WithMessage("شماره قرارداد باید بزرگ‌تر از صفر باشد.");

        RuleFor(x => x.RequestAmount)
            .GreaterThan(0)
            .When(x => x.RequestAmount.HasValue)
            .WithMessage("مبلغ درخواست باید بزرگ‌تر از صفر باشد.");

        RuleFor(x => x.ContractPath)
            .MaximumLength(500)
            .WithMessage("طول مسیر فایل قرارداد حداکثر می‌تواند ۵۰۰ کاراکتر باشد.");

        RuleFor(x => x.ProviderType)
            .IsInEnum()
            .WithMessage("نوع بانک نامعتبر است.");
    }
}

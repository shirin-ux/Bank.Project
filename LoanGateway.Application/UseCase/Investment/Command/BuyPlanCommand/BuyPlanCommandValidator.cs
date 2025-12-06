using FluentValidation;
using LoanService.Domain.Enum.Investment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlanCommand;

public sealed class BuyGoldPlanCommandValidator : AbstractValidator<BuyPlanCommand>
{
    private const long MinAmountRial = 1_000_000;

    public BuyGoldPlanCommandValidator()
    {
        RuleFor(x => x.PlanType)
            .Must(p => p == InvestmentPlanType.Gold || p == InvestmentPlanType.Silver)
            .WithMessage("در حال حاضر فقط طرح‌های طلا و نقره قابل خرید هستند.");

        RuleFor(x => x.AmountRial)
            .GreaterThanOrEqualTo(MinAmountRial)
            .WithMessage($"حداقل مبلغ سرمایه‌گذاری {MinAmountRial:N0} ریال است.");

        RuleFor(x => x.PlanType)
        .IsInEnum()
        .WithMessage("نوع طرح سرمایه‌گذاری نامعتبر است.");



        RuleFor(x => x.AcceptTerms)
            .Equal(true)
            .WithMessage("برای ادامه، باید شرایط خرید طرح را بپذیرید.");

        RuleFor(x => x.NationalCode)
            .NotEmpty()
            .Length(10)
            .WithMessage("کد ملی نامعتبر است.");
    }
}

using FluentValidation;
using LoanService.Domain.Enum.Investment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlan;

public sealed class BuyGoldPlanCommandValidator : AbstractValidator<BuyPlanCommand>
{
    private const long MinAmountRial = 1_000_000;

    public BuyGoldPlanCommandValidator()
    {
        RuleFor(x => x.PlanType)
            .Must(p => p == InvestmentPlanType.Gold || p == InvestmentPlanType.Silver)
            .WithMessage("در حال حاضر فقط طرح‌های طلا و نقره قابل خرید هستند.");


    }
}

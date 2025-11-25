using Common;
using LoanService.Domain.IRepository.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.GetInvestmentPlans
{
    public sealed class InvestmentPlansQueryHandler(IInvestmentPlanReadRepository investmentPlaRepo) : IRequestHandler<GetInvestmentPlansQuery, Result<List<InvestmentPlanResultDto>>>
    {
        private readonly IInvestmentPlanReadRepository _investmentPlaRepo= investmentPlaRepo;
        public async Task<Result<List<InvestmentPlanResultDto>>> Handle(GetInvestmentPlansQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var plans = await _investmentPlaRepo.GetActivePlansAsync(cancellationToken);
                if (plans == null || !plans.Any())
                {
                    return Result<List<InvestmentPlanResultDto>>.Failure(
                        new Error(404,"هیچ پلن سرمایه‌گذاری فعالی یافت نشد.")
                    );
                }
                var res = plans.Select(x => new InvestmentPlanResultDto
                {
                    Name = x.Name,
                    MinAmount = x.MinAmount,
                    PlanType = x.PlanType,
                    ShortDescription = x.ShortDescription
                }).ToList();
                return Result<List<InvestmentPlanResultDto>>.Success(res);
            }
            catch (Exception ex)
            {
                return Result<List<InvestmentPlanResultDto>>.Failure(
                    new Error(-500, "خطای غیرمنتظره‌ای رخ داد.")
                );
            }

        }
    }
}

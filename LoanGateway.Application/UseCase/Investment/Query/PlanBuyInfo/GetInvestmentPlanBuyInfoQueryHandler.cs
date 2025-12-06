using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.Exceptions;
using LoanService.Domain.IRepository.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.PlanBuyInfo
{
    public class GetInvestmentPlanBuyInfoQueryHandler : IRequestHandler<PlanBuyInfoQueryDto, Result<PlanBuyInfoResultDto>>
    {
        private readonly IInvestmentPlanReadRepository _planRepo;
        private readonly IInvestmentProvider _karizmahProvider;
        public GetInvestmentPlanBuyInfoQueryHandler(IInvestmentPlanReadRepository planRepo, IInvestmentProvider karizmahProvider)
        {
            _karizmahProvider = karizmahProvider;
            _planRepo = planRepo;
        }
        public async Task<Result<PlanBuyInfoResultDto>> Handle(PlanBuyInfoQueryDto request, CancellationToken cancellationToken)
        {
            var plan = await _planRepo.GetPlanAsync(request.PlanType.ToString(), cancellationToken);

            if (plan == null)
                throw new LogicException("طرحی  پیدا نشد");
            var res = new PlanBuyInfoResultDto
            {
                PlanType = plan.PlanType,
                Description = plan.Description,
                MinAmount = plan.MinAmount
            };
            var priceInfo = await _karizmahProvider.GetCurrentPriceAsync(request.PlanType, cancellationToken);
            var gramPrice = priceInfo.CurrentPrice;
        
            res.GramPrice = gramPrice;
            res.DailyChangePercent = priceInfo.DailyChangePercent;

            var grams = plan.MinAmount / gramPrice;
            var gramsRounded = Math.Floor(grams * 100m) / 100m;

            res.MinAmountEquivalentGram = gramsRounded;
        
            return Result<PlanBuyInfoResultDto>.Success(res);
        }
    }
}

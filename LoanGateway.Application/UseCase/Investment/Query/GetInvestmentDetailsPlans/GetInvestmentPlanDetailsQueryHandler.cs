using Common;
using LoanService.Application.Contracts;
using LoanService.Application.Exceptions;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;

public class GetInvestmentPlanDetailsQueryHandler
    : IRequestHandler<GetInvestmentPlanDetailsQuery, Result<InvestmentPlanDetailsResultDto>>
{
    private readonly IInvestmentPlanReadRepository _planRepo;
    private readonly IInvestmentProvider _karizmahProvider;

    public GetInvestmentPlanDetailsQueryHandler(
        IInvestmentPlanReadRepository planRepo,
        IInvestmentProvider karizmahProvider)
    {
        _planRepo = planRepo;
        _karizmahProvider = karizmahProvider;
    }

    public async Task<Result<InvestmentPlanDetailsResultDto>> Handle(
     GetInvestmentPlanDetailsQuery request,
     CancellationToken ct)
    {

        var plan = await _planRepo.GetPlanWithMetaAsync(request.PlanType, ct);
        if (plan is null)
            throw new NotFoundException("طرح سرمایه‌گذاری مورد نظر یافت نشد.");
        PlanSnapshotDto snapshot;
        IReadOnlyList<IndexPointDto> history;

        switch (request.PlanType)
        {
            case InvestmentPlanType.Gold:
            case InvestmentPlanType.Silver:
                {

                    snapshot = await _karizmahProvider.GetPlanSnapshotAsync(request.PlanType, ct);
                    history = await _karizmahProvider.GetPlanIndexHistoryAsync(request.PlanType, request.Range, ct);
                    break;
                }

            case InvestmentPlanType.FixedIncome:
                {

                    history = await _karizmahProvider
                        .GetPlanIndexHistoryAsync(request.PlanType, request.Range, ct);

                    snapshot = CalculateFixedIncomeSnapshot(request.PlanType, request.Range, history);
                    break;
                }

            default:
                throw new NotSupportedException($"نوع طرح '{request.PlanType}' پشتیبانی نمی‌شود.");
        }

        var res = new InvestmentPlanDetailsResultDto
        {
            PlanType = request.PlanType,
            PlanTitle = plan.Title,

            GramPrice = snapshot.GramPrice,
            DailyChangePercent = snapshot.DailyChangePercent,

            EffectiveAnnualRate = snapshot.EffectiveAnnualRate,
            ReturnFromStartPercent = snapshot.ReturnFromStartPercent,

            IndexHistory = history,

            Features = plan.Features
                .OrderBy(f => f.Order)
                .Select(f => new PlanFeatureDto { Title = f.Title, Description = f.Description })
                .ToList(),

            Faqs = plan.Faqs
                .OrderBy(f => f.Order)
                .Select(f => new FaqItemDto { Question = f.Question, Answer = f.Answer })
                .ToList()
        };

        return Result<InvestmentPlanDetailsResultDto>.Success(res);
    }
    private PlanSnapshotDto CalculateFixedIncomeSnapshot( InvestmentPlanType plan, InvestmentChartRange range, IReadOnlyList<IndexPointDto> history)
    {
        if (history == null || history.Count == 0)
            throw new InvalidOperationException("هیچ دیتای شاخصی برای طرح درآمد ثابت یافت نشد.");

        var ordered = history.OrderBy(p => p.Date).ToList();
        var first = ordered.First();
        var last = ordered.Last();

 
        decimal totalReturnPercent = 0m;
        if (first.IndexValue != 0)
        {
            var diff = last.IndexValue - first.IndexValue;
            totalReturnPercent = diff / first.IndexValue * 100m;
        }

        var totalDays = (last.Date.Date - first.Date.Date).TotalDays;
        decimal effectiveAnnualRate = 0m;

        if (totalDays > 0 && totalReturnPercent != 0)
        {
            var grossReturn = 1m + (totalReturnPercent / 100m);
            var annualized = Math.Pow((double)grossReturn, 365d / totalDays);
            effectiveAnnualRate = ((decimal)annualized - 1m) * 100m;
        }

        return new PlanSnapshotDto
        {
            PlanType = plan,
            GramPrice = 0,              
            DailyChangePercent = 0,        

            EffectiveAnnualRate = effectiveAnnualRate,      
            ReturnFromStartPercent = totalReturnPercent,
            LastUpdateUtc = last.Date.DateTime
        };
    }

}
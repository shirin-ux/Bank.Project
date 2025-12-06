using Common;
using LoanService.Application.Contracts;
using LoanService.Application.Exceptions;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.Exceptions;
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

        var historyRange = request.Range;

        if (request.PlanType == InvestmentPlanType.FixedIncome && (historyRange is InvestmentChartRange.OneHour or InvestmentChartRange.OneDay))
        {
            throw new LogicException("برای طرح درآمد ثابت فقط بازه‌های ۳، ۶ و ۹ ماهه مجاز است.");

        }
        IReadOnlyList<IndexPointDto> history;
        if (request.PlanType != InvestmentPlanType.FixedIncome)
        {
            history = await _karizmahProvider.GetPlanIndexHistoryAsync(request.PlanType, historyRange, ct, false);
        }
        else
        {
            history = GenerateFixedIncomeSyntheticHistory(historyRange, plan);
        }
        if (history is null || history.Count == 0)
            throw new InvalidOperationException("هیچ دیتای شاخصی برای این طرح یافت نشد.");

        PlanSnapshotDto snapshot = request.PlanType switch
        {
            InvestmentPlanType.FixedIncome => CalculateFixedIncomeSnapshot(request.PlanType, history),

            InvestmentPlanType.Gold or InvestmentPlanType.Silver => CalculateGoldSilverSnapshot(request.PlanType, history, historyRange),

            _ => throw new NotSupportedException($"نوع طرح '{request.PlanType}' پشتیبانی نمی‌شود.")
        };


        var res = new InvestmentPlanDetailsResultDto
        {
            PlanType = request.PlanType,

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
    private PlanSnapshotDto CalculateFixedIncomeSnapshot( InvestmentPlanType plan,  IReadOnlyList<IndexPointDto> history)
    {
        if (history == null || history.Count == 0)
            throw new InvalidOperationException("هیچ دیتای شاخصی برای طرح درآمد ثابت یافت نشد.");

        var ordered = history.Where(p => p.IndexValue > 0m).OrderBy(p => p.Date).ToList();
    
        if (!ordered.Any())
            throw new LogicException("هیچ دیتای معتبری برای طرح درآمد ثابت یافت نشد.");

        var first = ordered.First();
        var last = ordered.Last();

        decimal returnFromStartPercent = 0m;
        if (first.IndexValue != 0)
        {
            var diff = last.IndexValue - first.IndexValue;
            returnFromStartPercent = diff / first.IndexValue * 100m;
        }

        var totalDays = (last.Date - first.Date).TotalDays;
        decimal effectiveAnnualRate = 0m;

        if (totalDays > 0 && returnFromStartPercent != 0)
        {
            var grossReturn = 1m + (returnFromStartPercent / 100m);
            var annualized = Math.Pow((double)grossReturn, 365d / totalDays);
            effectiveAnnualRate = ((decimal)annualized - 1m) * 100m;
        }

        return new PlanSnapshotDto
        {
            PlanType = plan,
            GramPrice = 0,              
            DailyChangePercent = 0,        

            EffectiveAnnualRate = effectiveAnnualRate,      
            ReturnFromStartPercent = returnFromStartPercent,
            LastUpdateUtc = last.Date
        };
    }
    private PlanSnapshotDto CalculateGoldSilverSnapshot( InvestmentPlanType plan,IReadOnlyList<IndexPointDto> history,InvestmentChartRange renge)
    {
        try
        {

            if (history == null || history.Count == 0)
                throw new InvalidOperationException("هیچ دیتای شاخصی برای این طرح یافت نشد.");

            var ordered = history
                 .Where(p => p.IndexValue > 0m)
                .OrderBy(p => p.Date)
                .ToList();

            if (!ordered.Any())
                throw new InvalidOperationException("هیچ نقطه‌ی شاخص معتبری برای این طرح یافت نشد.");

            var first = ordered.First();

            var last = ordered.Last();


            var gramPrice = last.IndexValue;


            var lastDay = last.Date.Date;

            var previous = ordered.LastOrDefault(p => p.Date.Date < lastDay);// آخرین نقطه‌ی روز قبل

            decimal dailyChangePercent = 0m;
            if (previous is not null && previous.IndexValue != 0m)
            {
                var diff = gramPrice - previous.IndexValue;
                dailyChangePercent = diff / previous.IndexValue * 100m;
            }
            bool shoulComputeReturn = renge is InvestmentChartRange.ThreeMonths
                                           or InvestmentChartRange.SixMonths
                                           or InvestmentChartRange.OneYear;

            decimal returnFromStartPercent = 0m;
            decimal effectiveAnnualRate = 0m;
            if (shoulComputeReturn && first.IndexValue > 0)
            {
                var diffFromStart = gramPrice - first.IndexValue;
                returnFromStartPercent = diffFromStart / first.IndexValue * 100m;

                var totalDays = (last.Date - first.Date).TotalDays;
                if (totalDays > 0 && returnFromStartPercent != 0m)
                {
                    var grossReturn = 1m + (returnFromStartPercent / 100m);
                    var annualized = Math.Pow((double)grossReturn, 365d / totalDays);
                    effectiveAnnualRate = ((decimal)annualized - 1m) * 100m;
                }
            }
            return new PlanSnapshotDto
            {
                PlanType = plan,

                GramPrice = gramPrice,
                DailyChangePercent = dailyChangePercent,
                EffectiveAnnualRate = effectiveAnnualRate,
                ReturnFromStartPercent = returnFromStartPercent,
                LastUpdateUtc = last.Date
            };
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    private IReadOnlyList<IndexPointDto> GenerateFixedIncomeSyntheticHistory(InvestmentChartRange range, InvestmentPlans plan)
    {
        if (plan.FixedIncomeAnnualRatePercent is null || plan.FixedIncomeAnnualRatePercent <= 0)
                                      throw new LogicException("نرخ سالانه‌ی طرح درآمد ثابت تنظیم نشده است.");

        var annualRatePercent = plan.FixedIncomeAnnualRatePercent.Value; // مثلاً 23.5
        var r = annualRatePercent / 100m; // به نسبت

        var nowUtc = DateTime.UtcNow;
        var (fromUtc, toUtc) = range.ToDateRange(nowUtc);

        var startDate = fromUtc.Date;
        var endDate = toUtc.Date;

        var totalDays = (endDate - startDate).Days;
        if (totalDays <= 0)
            totalDays = 1;

        const decimal baseIndex = 100m;
        var points = new List<IndexPointDto>(totalDays + 1);

        for (int i = 0; i <= totalDays; i++)
        {
            var date = startDate.AddDays(i);
            var t = i / 365m; 
            var indexValue = baseIndex * (decimal)Math.Pow((double)(1 + r), (double)t);

            points.Add(new IndexPointDto
            {
                Date = date,
                IndexValue = indexValue
            });
        }

        return points.AsReadOnly();
    }




}
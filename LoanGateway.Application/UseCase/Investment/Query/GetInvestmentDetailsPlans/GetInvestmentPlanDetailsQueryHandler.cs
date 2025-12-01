using Common;
using LoanService.Application.Contracts;
using LoanService.Application.Exceptions;
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


        var snapshotTask = _karizmahProvider.GetPlanSnapshotAsync(request.PlanType, ct);
        var historyTask = _karizmahProvider.GetPlanIndexHistoryAsync(request.PlanType, request.Range, ct);

        await Task.WhenAll(snapshotTask, historyTask);

        var snapshot = await snapshotTask;
        var indexHistory = await historyTask ?? Array.Empty<IndexPointDto>();


        var res = new InvestmentPlanDetailsResultDto
        {
            PlanType = request.PlanType,
            PlanTitle = plan.Title,


            GramPrice = snapshot.GramPrice,
            DailyChangePercent = snapshot.DailyChangePercent,
            EffectiveAnnualRate = snapshot.EffectiveAnnualRate,
            ReturnFromStartPercent = snapshot.ReturnFromStartPercent,


            IndexHistory = indexHistory.ToList(),

            Features = plan.Features
                .OrderBy(f => f.Order)
                .Select(f => new PlanFeatureDto
                {
                    Title = f.Title,
                    Description = f.Description
                })
                .ToList(),

            Faqs = plan.Faqs
                .OrderBy(f => f.Order)
                .Select(f => new FaqItemDto
                {
                    Question = f.Question,
                    Answer = f.Answer
                })
                .ToList()
        };

        return Result<InvestmentPlanDetailsResultDto>.Success(res);
    }
}

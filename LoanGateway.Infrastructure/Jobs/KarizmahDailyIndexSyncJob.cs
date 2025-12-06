using Hangfire;
using Karizmah.Provider;
using Karizmah.Provider.Dtos;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using LoanService.Domain.IRepository.Loan;
using LoanService.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace LoanService.Infrastructure.Jobs;

public class KarizmahDailyIndexSyncJob(

                   IMediator mediator,
                   IInvestmentPlanReadRepository repo,
                   ILogger<KarizmahDailyIndexSyncJob> logger,
                   IKarizmahService investmentProvider,
                   IBackgroundJobClient bg) : IInvestmenJobRunner
{
    private readonly IBackgroundJobClient _bg = bg;
    private readonly IMediator _mediator = mediator;
    private readonly IInvestmentPlanReadRepository _repo = repo;
    private readonly IKarizmahService _investmentProvider = investmentProvider;
    private readonly ILogger<KarizmahDailyIndexSyncJob> _logger = logger;
    private const int MaxChunkDays = 15;
    private const int PageSize = 100000;
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var plans = new[]
                {
            InvestmentPlanType.Gold,
            InvestmentPlanType.Silver
        };
        var todayUtc = DateTime.UtcNow.Date;


        foreach (var plan in plans)
        {
            try
            {   

                var lastDate = await _repo.GetLastDateAsync(plan, cancellationToken);
                DateTime fromUtc;
                if (lastDate is null)
                {
               
                    fromUtc = todayUtc.AddDays(-364);
                }
                else
                {
                    fromUtc = lastDate.Value.AddDays(1);
                }
                if (fromUtc > todayUtc)
                {
                    _logger.LogInformation("دیتای اپدیت شده نداریم {Plan}", plan);
                    continue;
                }

                _logger.LogInformation("Syncing CHINDX for {Plan} from {From} to {To}", plan, fromUtc, todayUtc);

                var rawPoints = await FetchHistoryFromKarizmahAsync(plan, fromUtc, todayUtc, cancellationToken);

             
                var daily = rawPoints
                    .OrderBy(p => p.Timestamp)
                    .GroupBy(p => p.Timestamp.Date)
                    .Select(g => new InvestmentIndexHistory
                    {
                        PlanType = plan,
                        IndexDateTimeUtc = g.Key,
                        IndexValue = g.Last().Value
                    })
                    .ToList();

                if (daily.Count > 0)
                {
                    await _repo.UpsertDailyHistoryAsync(plan, daily, cancellationToken);
                }

       
                var cutoff = todayUtc.AddDays(-364);
                await _repo.DeleteOlderThanAsync(plan, cutoff, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing CHINDX history for {Plan}", plan);
            }
        }
    }
    private async Task<List<ChindxIndexPointDto>> FetchHistoryFromKarizmahAsync(InvestmentPlanType plan, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var iranOffset = TimeSpan.FromHours(3.5);
        var fromLocal = new DateTimeOffset(fromUtc, TimeSpan.Zero).ToOffset(iranOffset);
        var toLocal = new DateTimeOffset(toUtc, TimeSpan.Zero).ToOffset(iranOffset);

        var allPoints = new List<ChindxIndexPointDto>();
        var instrumentId = InvestmentPlanTypeExtensions.ToInstrumentId(plan);

        var cursorFrom = fromLocal;
        while (cursorFrom <= toLocal)
        {
            var chunkTo = cursorFrom.AddDays(MaxChunkDays - 1);
            if (chunkTo > toLocal)
                chunkTo = toLocal;

            var offset = 0;
            while (true)
            {
                var req = new ChindxIndexValueRequestDto
                {
                    InstrumentId = instrumentId,
                    FromDateKey = cursorFrom.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    ToDateKey = chunkTo.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    FromDate = cursorFrom,
                    ToDate = chunkTo,
                    Size = PageSize,
                    Offset = offset
                };

                var res = await _investmentProvider.GetGoldIndexAsync(req, ct);
                var page = res.Data?.IndexValue;

                if (page == null || page.Count == 0)
                    break;

                allPoints.AddRange(page);

                if (page.Count < PageSize)
                    break;

                offset += PageSize;
                await Task.Delay(200, ct);
            }

            cursorFrom = chunkTo.AddDays(1);
        }

        return allPoints;
    }
}



using Hangfire;
using Karizmah.Provider;
using Karizmah.Provider.Dtos;
using LoanService.Application.Contracts;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using MediatR;
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
    private const int MaxChunkDays = 3;
    private const int PageSize = 10;


    private readonly TimeSpan IranOffset = TimeSpan.FromHours(3.5);
    private DateTime IranTodayDate() => DateTime.UtcNow.Add(IranOffset).Date;
    private DateTime IranYesterdayDate() => IranTodayDate().AddDays(-1);
    private DateTime DbDayKey(DateTime iranDate) => iranDate.Date;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var plans = new[]
                {
            InvestmentPlanType.Gold,
            InvestmentPlanType.Silver
        };
        var toIran = IranYesterdayDate();
        var toKey = DbDayKey(toIran);

        foreach (var plan in plans)
        {
            try
            {

                var lastKey = await _repo.GetLastDateAsync(plan, ct);

                DateTime fromKey;
                if (lastKey is null)
                {

                    const int BootstrapDays = 7;
                    fromKey = DbDayKey(toIran.AddDays(-(BootstrapDays - 1)));
                }
                else
                {
                    fromKey = DbDayKey(lastKey.Value).AddDays(1);
                }

                if (fromKey > toKey)
                {
                    _logger.LogInformation("No new daily data for {Plan}", plan);
                    continue;
                }

                _logger.LogInformation("Sync daily CHINDX {Plan} from {From} to {To}", plan, fromKey, toKey);

                var rawPoints = await FetchHistoryFromKarizmahAsync(plan, fromKey, toKey, ct);
                var daily = rawPoints
                .Where(p => p.Value > 0m)
                .OrderBy(p => p.Timestamp)
                .GroupBy(p => p.Timestamp.ToOffset(IranOffset).Date)
                .Select(g =>
                {
                    var last = g.Last();
                    return new InvestmentIndexHistory
                    {
                        PlanType = plan,
                        IndexDateTimeUtc = DbDayKey(g.Key),
                        IndexValue = last.Value
                    };
                })
                .ToList();

                if (daily.Count > 0)
                    await _repo.UpsertDailyHistoryAsync(plan, daily, ct);

                var cutoff = DbDayKey(toIran.AddDays(-364));
                await _repo.DeleteOlderThanAsync(plan, cutoff, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing CHINDX history for {Plan}", plan);
            }
        }
    }
    private async Task<List<ChindxIndexPointDto>> FetchHistoryFromKarizmahAsync(InvestmentPlanType plan,
    DateTime fromDayKey,
    DateTime toDayKey,
    CancellationToken ct)
    {
        var instrumentId = InvestmentPlanTypeExtensions.ToInstrumentId(plan);
        var all = new List<ChindxIndexPointDto>();

        var cursor = fromDayKey.Date;
        var end = toDayKey.Date;

        while (cursor <= end)
        {
            var chunkEnd = cursor.AddDays(MaxChunkDays - 1);
            if (chunkEnd > end) chunkEnd = end;

            var offset = 0;
            while (true)
            {

                var fromLocal = new DateTimeOffset(cursor, IranOffset);
                var toLocal = new DateTimeOffset(chunkEnd.AddDays(1).AddTicks(-1), IranOffset);

                var req = new ChindxIndexValueRequestDto
                {
                    InstrumentId = instrumentId,
                    FromDateKey = cursor.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    ToDateKey = chunkEnd.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    FromDate = fromLocal,
                    ToDate = toLocal,
                    Size = PageSize,
                    Offset = offset
                };

                var res = await _investmentProvider.GetGoldIndexAsync(req, ct);
                var page = res.Data?.IndexValue;

                if (page == null || page.Count == 0) break;

                all.AddRange(page);

                if (page.Count < PageSize) break;

                offset += PageSize;
                await Task.Delay(200, ct);
            }

            cursor = chunkEnd.AddDays(1);
            await Task.Delay(200, ct);
        }

        return all;
    }
}




using Karizmah.Provider;
using Karizmah.Provider.Dtos;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Domain.Enum;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LoanService.Infrastructure.Services;

public class KarizmahInvestmentProvider(IKarizmahService client, IMemoryCache cache) : IProviderBase, IInvestmentProvider
{
    public ProviderType ProviderType => ProviderType.Karizmah;
    private readonly IKarizmahService _client = client;
    private readonly IMemoryCache _cache= cache;
    public async Task<IReadOnlyList<IndexPointDto>> GetPlanIndexHistoryAsync(
       InvestmentPlanType plan,
       InvestmentChartRange range,
       CancellationToken ct)
    {
        var cacheKey = $"chindx:{plan}:{range}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<IndexPointDto> cached))
            return cached;
        var data = await FetchHistoryFromKarizmahAsync(plan, range, ct);

        var result = data
            .OrderBy(p => p.Timestamp)
            .Select(p => new IndexPointDto
            {
                Date = p.Timestamp,
                IndexValue = p.Value
            })
            .ToList()
            .AsReadOnly();
        var ttl = range is InvestmentChartRange.OneHour or InvestmentChartRange.OneDay
        ? TimeSpan.FromMinutes(1)
        : TimeSpan.FromMinutes(10);

        _cache.Set(cacheKey, data, ttl);
        return result;
    }



    private const int MaxChunkDays = 15;
    private const int PageSize = 500; // اگر واقعاً کاریزما اجازه بده؛
                                      // اگر نه، بذار همون 10 که گفته بودند.

    private async Task<List<ChindxIndexPointDto>> FetchHistoryFromKarizmahAsync(
        InvestmentPlanType plan,
        InvestmentChartRange range,
        CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var (fromUtc, toUtc) = range.ToDateRange(nowUtc);

        var iranOffset = TimeSpan.FromHours(3.5);
        var fromLocal = new DateTimeOffset(fromUtc, TimeSpan.Zero).ToOffset(iranOffset);
        var toLocal = new DateTimeOffset(toUtc, TimeSpan.Zero).ToOffset(iranOffset);

        var allPoints = new List<ChindxIndexPointDto>();
        var instrumentId = InvestmentPlanTypeExtensions.ToInstrumentId(plan);

        var cursorFrom = fromLocal;
        while (cursorFrom < toLocal)
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

                var res = await _client.GetGoldIndexAsync(req, ct);
                var page = res.Data?.IndexValue;

                if (page == null || page.Count == 0)
                    break;

                allPoints.AddRange(page);

                if (page.Count < PageSize)
                    break;

                offset += PageSize;
                if (offset > 1000)
                    break;
            }

            cursorFrom = chunkTo.AddSeconds(1);
        }

        return allPoints;
    }

}

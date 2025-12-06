using Azure.Core;
using Common;
using Karizmah.Provider;
using Karizmah.Provider.Dtos;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Command.BuyPlanCommand;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Application.UseCase.Investment.Query.PlanBuyInfo;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.Exceptions;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography.Pkcs;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LoanService.Infrastructure.Services;

public class KarizmahInvestmentProvider(
    IInvestmentPlanReadRepository investmentPlanReadRepository,
    ILogger<KarizmahInvestmentProvider> logger,
    IKarizmahService client,
    IMemoryCache cache) : IProviderBase, IInvestmentProvider
{
    private readonly IInvestmentPlanReadRepository _investmentPlanReadRepository = investmentPlanReadRepository;
    public ProviderType ProviderType => ProviderType.Karizmah;
    private readonly IKarizmahService _client = client;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<KarizmahInvestmentProvider> _logger = logger;
    private static readonly TimeSpan LiveTtl = TimeSpan.FromMinutes(1);
    private const int MaxChunkDays = 15;
    private const int PageSize = 10;

    public async Task<IReadOnlyList<IndexPointDto>> GetPlanIndexHistoryAsync(InvestmentPlanType plan, InvestmentChartRange range, CancellationToken ct, bool forceRefresh = false)
    {
        var nowUtc = DateTime.UtcNow;
        var (fromUtc, toUtc) = range.ToDateRange(nowUtc);
        var cacheKey = $"chindx:{plan}:{range}";


        if (range is InvestmentChartRange.OneDay or InvestmentChartRange.OneHour)
        {
            if (!forceRefresh && _cache.TryGetValue(cacheKey, out IReadOnlyList<IndexPointDto> cachedShort))
                return cachedShort;

            var raw = await FetchHistoryFromKarizmahAsync(plan, range, ct); // ورژن قبلی که بر اساس range کار می‌کرد

            var result = raw
                .OrderBy(p => p.Timestamp)
                .Select(p => new IndexPointDto
                {
                    Date = p.Timestamp,
                    IndexValue = p.Value
                })
                .ToList()
                .AsReadOnly();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(1));
            return result;
        }


        if (!forceRefresh && _cache.TryGetValue(cacheKey, out IReadOnlyList<IndexPointDto> cachedLong))
            return cachedLong;

        var dbHistory = await _investmentPlanReadRepository.GetRangeAsync(plan, fromUtc, toUtc, ct);

        if (dbHistory == null || dbHistory.Count == 0)
            throw new InvalidOperationException("برای این بازه هنوز داده‌ای در سیستم ثبت نشده است.");

        var resultFromDb = dbHistory
            .OrderBy(x => x.IndexDateTimeUtc)
            .Select(x => new IndexPointDto
            {
                Date = x.IndexDateTimeUtc,
                IndexValue = x.IndexValue
            })
            .ToList()
            .AsReadOnly();

        _cache.Set(cacheKey, resultFromDb, TimeSpan.FromHours(1));
        return resultFromDb;
    }
    private async Task<List<ChindxIndexPointDto>> FetchHistoryFromKarizmahAsync(InvestmentPlanType plan, InvestmentChartRange range, CancellationToken ct)
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

            }

            cursorFrom = chunkTo.AddSeconds(1);
        }

        return allPoints;
    }

    public async Task<PlanPriceInfoDto> GetCurrentPriceAsync(InvestmentPlanType planType, CancellationToken ct)
    {

        var instrumentId = planType switch
        {
            InvestmentPlanType.Gold => "IRTICHGOLD01",
            InvestmentPlanType.Silver => "IRTICHSILV01",
            _ => throw new NotSupportedException($"Plan '{planType}' برای این طرح پشتیبانی نمی‌شود.")
        };

        if (planType is not InvestmentPlanType.Gold and not InvestmentPlanType.Silver)
            throw new NotSupportedException($"Plan '{planType}' برای این طرح  پشتیبانی نمی‌شود.");

        var nowUtc = DateTime.UtcNow;
        var fromUtc = nowUtc.AddDays(-1);


        var iranOffset = TimeSpan.FromHours(3.5);
        var fromLocal = new DateTimeOffset(fromUtc, TimeSpan.Zero).ToOffset(iranOffset);
        var toLocal = new DateTimeOffset(nowUtc, TimeSpan.Zero).ToOffset(iranOffset);

        var resKarizmah = await _client.GetGoldIndexAsync(new ChindxIndexValueRequestDto
        {

            FromDate = fromLocal,
            ToDate = toLocal,
            InstrumentId = instrumentId,
            FromDateKey = fromLocal.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            ToDateKey = toLocal.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            Offset = 0,
            Size = 10
        });

        var lastTwoDays = await _investmentPlanReadRepository.GetLatestPointsAsync(planType, 2, ct);
        var points = resKarizmah.Data?.IndexValue;


        var ordered = points.Where(p => p.Value > 0m).OrderBy(p => p.Timestamp).ToList();
        if (ordered.Count == 0)
            throw new ExternalServiceException("لیست شاخص کاریزما خالی یا نامعتبر است.", 0, null);

        var lastPoint = ordered[^1];

        var pricePerGram = lastPoint.Value * 10m;

        decimal dailyChangePercent = 0m;
        if (lastTwoDays.Count >= 2)
        {
            var todayRow = lastTwoDays[0]; // آخرین تاریخ
            var previousRow = lastTwoDays[1]; // روز قبل

            if (previousRow.IndexValue > 0m)
            {
                var diff = todayRow.IndexValue - previousRow.IndexValue;
                dailyChangePercent = diff / previousRow.IndexValue * 100m;
            }
        }

        var res = new PlanPriceInfoDto
        {
            CurrentPrice = pricePerGram,
            DailyChangePercent = dailyChangePercent,
            LastUpdateUtc = lastPoint.Timestamp
        };
        return res;


    }

    public async Task<BuyPlanResultDto> CreatePolicyAndBuyAsync(BuyPlanCommand cmd, CancellationToken ct)
    {


        var req = new KarizmahOrderBuyRequestDto
        {
            amount = cmd.AmountRial,
            birthDate = cmd.BirthDate.ToKarizmahBirthDate(),
            callbackUrl = cmd.CallbackUrl,
            planTypeAliasName = cmd.PlanType.ToString(),
            nationalCode = cmd.NationalCode,
            address=cmd.PaymentUrl
        };
        var resKarizmah =await _client.BuyOrderAsync(req, ct);
        return new BuyPlanResultDto
        {
            AmountRial = cmd.AmountRial,
            TraceId = resKarizmah.data.traceId,
            PlanType = cmd.PlanType,
            NationalCode = resKarizmah.data.nationalCode,
            BirthDate=resKarizmah.data.birthDate
           
        };

    }
}

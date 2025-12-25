using Common;
using Karizmah.Provider;
using Karizmah.Provider.Dtos;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Command.BuyPlan;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Application.UseCase.Investment.Query.OrderBuy;
using LoanService.Application.UseCase.Investment.Query.PlanBuyInfo;
using LoanService.Domain.Enum;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.Exceptions;
using LoanService.Domain.IRepository.Investment;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;

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

    private const int PageSize = 100;

    private readonly TimeSpan IranOffset = TimeSpan.FromHours(3.5);

    private DateTimeOffset IranNow() =>
        new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToOffset(IranOffset);

    private DateTime IranYesterdayKey() =>
        IranNow().Date.AddDays(-1);


    public async Task<IReadOnlyList<IndexPointDto>> GetPlanIndexHistoryAsync(InvestmentPlanType plan,
        InvestmentChartRange range, 
        CancellationToken ct, 
        bool forceRefresh = false,
         bool isMinute = false)
    {
        var nowUtc = DateTime.UtcNow;
        //var (fromUtc, toUtc) = range.ToDateRange(nowUtc);

        var (fromUtc, toUtc) = isMinute
      ?  BoxStatusExtensions.ToDateRange(InvestmentBoxStatus.Minute, plan, range, nowUtc)
      : BoxStatusExtensions.ToDateRange(InvestmentBoxStatus.Daily, plan, range, nowUtc);


        var cacheKey = $"chindx:{plan}:{range}:{(isMinute ? "min" : "day")}";

       // InvestmentRangeRules.EnsureAllowed(plan, range);

        if (range is InvestmentChartRange.OneDay or InvestmentChartRange.OneHour || isMinute)
        {
            if (!forceRefresh && _cache.TryGetValue(cacheKey, out IReadOnlyList<IndexPointDto> cachedShort))
                return cachedShort;

            var nowIran = IranNow();
            DateTimeOffset fromLocal, toLocal;

            if (isMinute /*|| range == InvestmentChartRange.OneHour*/)
            {
                fromLocal =   nowIran.AddMinutes(-60);
                toLocal = nowIran;
            }
            else // OneDay
            {
                fromLocal = new DateTimeOffset(nowIran.Date, IranOffset);
                toLocal = nowIran;
            }



            //if (range == InvestmentChartRange.OneHour)
            //{
            //    fromLocal = nowIran.AddHours(-1);
            //    toLocal = nowIran;
            //}
            //else
            //{

            //    fromLocal = new DateTimeOffset(nowIran.Date, IranOffset);
            //    toLocal = nowIran;
            //}
            var raw = await FetchHistoryFromKarizmahAsync(plan, fromLocal, toLocal, ct);

            var result = raw
                 .Where(p => p.Value > 0m)
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
        var toKey = IranYesterdayKey();
        var fromKey = range switch
        {

            InvestmentChartRange.ThreeMonths => toKey.AddDays(-89),
            InvestmentChartRange.SixMonths => toKey.AddDays(-179),
            InvestmentChartRange.OneYear => toKey.AddDays(-364),
            _ => throw new NotSupportedException($"Range '{range}' پشتیبانی نمی‌شود.")
        };
        var dbHistory = await _investmentPlanReadRepository.GetRangeAsync(plan, fromUtc, toUtc, ct);

        if (dbHistory == null || dbHistory.Count == 0)
            throw new InvalidOperationException("برای این بازه هنوز داده‌ای در سیستم ثبت نشده است.");

        var resultFromDb = dbHistory
            .OrderBy(x => x.IndexDateTimeUtc)
            .Select(x => new IndexPointDto
            {
                Date = new DateTimeOffset(x.IndexDateTimeUtc.Date, IranOffset),
                IndexValue = x.IndexValue
            })
            .ToList()
            .AsReadOnly();

        _cache.Set(cacheKey, resultFromDb, TimeSpan.FromHours(1));
        return resultFromDb;
    }
    private async Task<List<ChindxIndexPointDto>> FetchHistoryFromKarizmahAsync(
        InvestmentPlanType plan,
         DateTimeOffset fromLocal,
         DateTimeOffset toLocal,
        CancellationToken ct)
    {

        var allPoints = new List<ChindxIndexPointDto>();


        var instrumentId = InvestmentPlanTypeExtensions.ToInstrumentId(plan);
        var offset = 0;


        while (true)
        {
            var req = new ChindxIndexValueRequestDto
            {
                InstrumentId = instrumentId,

          
                FromDateKey = fromLocal.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                ToDateKey = toLocal.ToString("yyyyMMdd", CultureInfo.InvariantCulture),

                FromDate = fromLocal,
                ToDate = toLocal,

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
            await Task.Delay(150, ct);

        }



        return allPoints;
    }

    public async Task<PlanPriceInfoDto> GetCurrentPriceAsync(InvestmentPlanType planType, CancellationToken ct)
    {
        var cacheKey = $"chindx:price:{planType}";

        if (_cache.TryGetValue(cacheKey, out PlanPriceInfoDto cached))
            return cached;

        var instrumentId = InvestmentPlanTypeExtensions.ToInstrumentId(planType);

        if (planType is not InvestmentPlanType.Gold and not InvestmentPlanType.Silver)
            throw new NotSupportedException($"Plan '{planType}' برای این طرح  پشتیبانی نمی‌شود.");
        var nowIran = IranNow();
        var fromLocal = new DateTimeOffset(nowIran.Date, IranOffset);
        var toLocal = nowIran;

        var latest = await FetchLatestPointAsync(instrumentId, fromLocal, toLocal, ct);
        if (latest == null)
        {
            fromLocal = nowIran.AddHours(-24);
            latest = await FetchLatestPointAsync(instrumentId, fromLocal, toLocal, ct);
        }

        if (latest == null || latest.Value <= 0m)
            throw new LogicException("لیست شاخص کاریزما خالی یا نامعتبر است.");
        var currentIndex = latest.Value;
        var pricePerGram = currentIndex * 10m;
        var yesterdayKey = IranYesterdayKey();

        var lastTwo = await _investmentPlanReadRepository.GetLatestPointsAsync(planType, 2, ct);
        decimal dailyChangePercent = 0m;


        var yesterday = lastTwo.FirstOrDefault();
        if (yesterday != null && yesterday.IndexValue > 0m)
        {
            dailyChangePercent = (currentIndex - yesterday.IndexValue) / yesterday.IndexValue * 100m;
        }

        var res = new PlanPriceInfoDto
        {
            CurrentPrice = pricePerGram,
            DailyChangePercent = dailyChangePercent,
            LastUpdateUtc = latest.Timestamp
        };
        
        // ذخیره در cache برای 1 دقیقه
        _cache.Set(cacheKey, res, TimeSpan.FromMinutes(1));
        
        return res;
    }

    public async Task<Result<BuyPlanResultDto>> CreatePolicyAndBuyAsync(BuyPlanCommand cmd,  string birthDate, string postalCode, string nationalCode, CancellationToken ct)
    {
        _logger.LogInformation("در حال ایجاد Policy برای کاربر با کد ملی {NationalCode}, PlanType: {PlanType}", 
            nationalCode, cmd.PlanType);

        var req = new KarizmahCreatePolicyWithoutInitialPaymentRequestDto
        {
            birthDate = birthDate,
            planTypeAliasName =  cmd.PlanType.ToString(),
            nationalCode = nationalCode,
            postalCode= postalCode,
            age=null,
            coefficient=null,
            coverageAliasName=null,
            address=null,
            description = null
        };
        
        var resKarizmah = await _client.CreatePolicyWithoutInitialPaymentAsync(req, ct);

        if (resKarizmah is null)
        {
            _logger.LogError("پاسخی از سرویس کاریزما دریافت نشد برای کاربر {NationalCode}", nationalCode);
            throw new InvalidOperationException("پاسخی از سرویس دریافت نشد.");
        }

        if (!resKarizmah.isSuccess || resKarizmah.data is null)
        {
            var errorMessage = resKarizmah.errorMessages != null && resKarizmah.errorMessages.Any()
                ? string.Join(". ", resKarizmah.errorMessages.Select(e => e.message?.Trim()).Where(m => !string.IsNullOrWhiteSpace(m)))
                : "خطا در ایجاد صدور بیمه‌نامه.";
            
            // اطمینان از اینکه پیام با نقطه تمام می‌شود
            if (!string.IsNullOrWhiteSpace(errorMessage) && !errorMessage.EndsWith(".") && !errorMessage.EndsWith("!") && !errorMessage.EndsWith("?"))
            {
                errorMessage += ".";
            }
            
            _logger.LogWarning("خطا در ایجاد Policy از سرویس کاریزما برای کاربر {NationalCode}. خطا: {ErrorMessage}", 
                nationalCode, errorMessage);

            return Result<BuyPlanResultDto>.Failure(
                new Error(
                    Code: 200,
                    Message: errorMessage
                ));
        }

        var result = new BuyPlanResultDto
        {
            TraceId = resKarizmah.data.traceId,
            PlanType = cmd.PlanType,
            OrderId = resKarizmah.data.id,
            ProviderPolicyId = resKarizmah.data.id, // id همان PolicyId است
            NationalCode = nationalCode,
            BirthDate = birthDate,
            IsRepeated = resKarizmah.data.isRepeated
        };

        return Result<BuyPlanResultDto>.Success(result);
    }

    public Task<BuyPlanResultDto> GetRevokableAmountAsync(BuyPlanCommand cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    private async Task<ChindxIndexPointDto?> FetchLatestPointAsync(
    string instrumentId,
    DateTimeOffset fromLocal,
    DateTimeOffset toLocal,
    CancellationToken ct)
    {
        var offset = 0;
        ChindxIndexPointDto? best = null;

        DateTimeOffset? firstTs = null;
        DateTimeOffset? lastTs = null;
        bool? isAscending = null;

        while (true)
        {
            var req = new ChindxIndexValueRequestDto
            {
                InstrumentId = instrumentId,
                FromDateKey = fromLocal.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                ToDateKey = toLocal.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                FromDate = fromLocal,
                ToDate = toLocal,
                Size = PageSize,
                Offset = offset
            };

            var res = await _client.GetGoldIndexAsync(req, ct);
            var page = res.Data?.IndexValue;

            if (page == null || page.Count == 0)
                break;

            var ordered = page.Where(x => x.Value > 0m).OrderBy(x => x.Timestamp).ToList();
            if (ordered.Count > 0)
            {
                best = best == null
                    ? ordered[^1]
                    : (ordered[^1].Timestamp > best.Timestamp ? ordered[^1] : best);

                firstTs ??= ordered[0].Timestamp;
                lastTs = ordered[^1].Timestamp;

                if (isAscending == null && firstTs != null && lastTs != null)
                    isAscending = lastTs > firstTs;
            }


            if (isAscending == false)
                break;

            if (page.Count < PageSize)
                break;

            offset += PageSize;
            await Task.Delay(120, ct);
        }

        return best;
    }

    public async Task<GetOrderStatusResultDto> GetOrderBuyByIdAsync(Guid Id, CancellationToken ct)
    {

 
        var resKarizmah = await _client.GetOrderBuyByIdAsync(Id, ct);

        if (resKarizmah is null)
            throw new InvalidOperationException("پاسخی از سرویس دریافت نشد.");

        if (!resKarizmah.isSuccess || resKarizmah.data is null)
        {

            var msg = resKarizmah.errorMessages != null && resKarizmah.errorMessages.Any()
                ? string.Join(" | ", resKarizmah.errorMessages.Select(e => e.message))
                : "خطا در استعلام صدور بیمه‌نامه .";

            throw new InvalidOperationException(msg);
        }
        if (!Enum.TryParse<statusType>(resKarizmah.data.status.ToString(), ignoreCase: true, out var statusEnum))
        {
            throw new InvalidOperationException($"وضعیت نامعتبر از سرویس دریافت شد: {resKarizmah.data.status}");
        }
    

        return new GetOrderStatusResultDto
        {
             Status = statusEnum,
             UliStatus=resKarizmah.data.uliStatus,
             WealthPolicyId = resKarizmah.data.wealthPolicyId
        };
    }

    public async Task<Result<IncreaseCapitalDirectResultDto>> IncreaseCapitalDirectAsync(
        long wealthPolicyId, 
        decimal amount, 
        string receiptNumber, 
        DateTime receiptDate, 
        string? description, 
        CancellationToken ct)
    {
        _logger.LogInformation("در حال افزایش سرمایه برای wealthPolicyId: {WealthPolicyId}, Amount: {Amount}", 
            wealthPolicyId, amount);

        var req = new KarizmahIncreaseCapitalDirectRequestDto
        {
            policyId = wealthPolicyId,
            amount = amount,
            receiptNumber = receiptNumber,
            rceiptDate = receiptDate,
            description = description
        };

        var resKarizmah = await _client.IncreaseCapitalDirectAsync(req, ct);

        if (resKarizmah is null)
        {
            _logger.LogError("پاسخی از سرویس کاریزما دریافت نشد برای افزایش سرمایه wealthPolicyId: {WealthPolicyId}", 
                wealthPolicyId);
            throw new InvalidOperationException("پاسخی از سرویس دریافت نشد.");
        }

        if (!resKarizmah.isSuccess || resKarizmah.data is null)
        {
            var errorMessage = resKarizmah.errorMessages != null && resKarizmah.errorMessages.Any()
                ? string.Join(". ", resKarizmah.errorMessages.Select(e => e.message?.Trim()).Where(m => !string.IsNullOrWhiteSpace(m)))
                : "خطا در افزایش سرمایه.";
            
            // اطمینان از اینکه پیام با نقطه تمام می‌شود
            if (!string.IsNullOrWhiteSpace(errorMessage) && !errorMessage.EndsWith(".") && !errorMessage.EndsWith("!") && !errorMessage.EndsWith("?"))
            {
                errorMessage += ".";
            }
            
            _logger.LogWarning("خطا در افزایش سرمایه از سرویس کاریزما برای wealthPolicyId: {WealthPolicyId}. خطا: {ErrorMessage}", 
                wealthPolicyId, errorMessage);

            return Result<IncreaseCapitalDirectResultDto>.Failure(
                new Error(
                    Code: 200,
                    Message: errorMessage
                ));
        }

        var result = new IncreaseCapitalDirectResultDto
        {
            TraceId = resKarizmah.data.traceId,
            OrderId = resKarizmah.data.Id,
            IsRepeated = resKarizmah.data.isRepeated
        };

        _logger.LogInformation("افزایش سرمایه با موفقیت انجام شد. wealthPolicyId: {WealthPolicyId}, TraceId: {TraceId}, OrderId: {OrderId}",
            wealthPolicyId, result.TraceId, result.OrderId);

        return Result<IncreaseCapitalDirectResultDto>.Success(result);
    }
}

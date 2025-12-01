using Bank.Mellat.Provider.Dtos;
using Karizmah.Provider;
using Karizmah.Provider.Dtos;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Application.UseCase.Loan.Command.DepositRequest;
using LoanService.Domain.Enum;
using LoanService.Domain.Enum.Investment;
using System.Diagnostics.Metrics;
using System.Globalization;

namespace LoanService.Infrastructure.Services;

public class KarizmahInvestmentProvider(IKarizmahService client) : IProviderBase,IInvestmentProvider
{
    public ProviderType ProviderType => ProviderType.Karizmah;
    private readonly IKarizmahService _client= client;
    public async Task<IReadOnlyList<IndexPointDto>> GetPlanIndexHistoryAsync(InvestmentPlanType plan, InvestmentChartRange range, CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var (fromUtc, toUtc) = range.ToDateRange(nowUtc);


        var iranOffset = TimeSpan.FromHours(3.5);
        var fromLocal = new DateTimeOffset(fromUtc, TimeSpan.Zero).ToOffset(iranOffset);
        var toLocal = new DateTimeOffset(toUtc, TimeSpan.Zero).ToOffset(iranOffset);

     
        var fromKey = fromLocal.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var toKey = toLocal.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        var allPoints = new List<ChindxIndexPointDto>();
        var instrumentId = InvestmentPlanTypeExtensions.ToInstrumentId(plan);

        var cursorFrom = fromLocal;

        while (cursorFrom < toLocal)
        {
            var cursorTo = cursorFrom.AddDays(9);
            if (cursorTo > toLocal)
                cursorTo = toLocal;

            var req = new ChindxIndexValueRequestDto
            {
                InstrumentId = instrumentId,
                FromDateKey = cursorFrom.ToString("yyyyMMdd"),
                ToDateKey = cursorTo.ToString("yyyyMMdd"),
                FromDate = cursorFrom,
                ToDate = cursorTo,
                Size = 10,
                Offset = 0
            };

            var res = await _client.GetGoldIndexAsync(req, ct);

            if (res.Data?.IndexValue != null)
                allPoints.AddRange(res.Data.IndexValue);

            // حرکت به تکه بعدی (فاصله‌ها تداخل نداشته باشند)
            cursorFrom = cursorTo.AddDays(1);
        }

        // map به DTO خودت
        var result = allPoints
            .OrderBy(p => p.Timestamp)
            .Select(p => new IndexPointDto
            {
                Date = p.Timestamp.LocalDateTime,
                IndexValue = p.Value
            })
            .ToList()
            .AsReadOnly();

        return result;
    }

    public async Task<PlanSnapshotDto> GetPlanSnapshotAsync(InvestmentPlanType plan, CancellationToken ct)
    {
        var history = await GetPlanIndexHistoryAsync(plan, InvestmentChartRange.OneYear, ct);

        if (history is null || history.Count == 0)
            throw new InvalidOperationException("هیچ دیتای شاخصی برای این طرح یافت نشد.");

     
        var ordered = history.OrderBy(p => p.Date).ToList();

        // آخرین نقطه‌ی دوره (آخرین قیمت / شاخص)
        var last = ordered[^1];

        // اولین نقطه‌ی دوره
        var first = ordered[0];

        var gramPrice = last.IndexValue;


        //  محاسبه درصد تغییر روزانه نسبت به روز قبل
        var todayDate = last.Date.Date;


        var previous = ordered.LastOrDefault(p => p.Date.Date < todayDate);
        decimal dailyChangePercent = 0m;


        if (previous is not null && previous.IndexValue != 0)
        {
            var diff = gramPrice - previous.IndexValue;
            dailyChangePercent = diff / previous.IndexValue * 100m;
        }

        // ۴) بازده از ابتدای دوره (اولین نقطه تا آخرین نقطه)
        decimal returnFromStartPercent = 0m;

        if (first.IndexValue != 0)
        {
            var diffFromStart = gramPrice - first.IndexValue;
            returnFromStartPercent = diffFromStart / first.IndexValue * 100m;
        }
        // ۵) نرخ موثر سالانه (annualized) از روی همین بازده و طول دوره
        decimal effectiveAnnualRate = 0m;

        var totalDays = (last.Date.Date - first.Date.Date).TotalDays;

        if (totalDays > 0 && returnFromStartPercent != 0)
        {
            // grossReturn = 1 + (return% / 100)
            var grossReturn = 1m + (returnFromStartPercent / 100m); // مثلا 1.20 برای 20٪

            // annualized = gross^(365 / days)
            var annualized = Math.Pow((double)grossReturn, 365d / totalDays);

            effectiveAnnualRate = ((decimal)annualized - 1m) * 100m;
        }


        var snapshot = new PlanSnapshotDto
        {
            GramPrice = gramPrice,
            DailyChangePercent = dailyChangePercent,
            EffectiveAnnualRate = effectiveAnnualRate,
            ReturnFromStartPercent = returnFromStartPercent
        };

        return snapshot;
    }
}

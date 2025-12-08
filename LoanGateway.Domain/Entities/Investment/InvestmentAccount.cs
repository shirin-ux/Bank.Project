using LoanService.Domain.Enum.Investment;
using LoanService.Domain.Exceptions;

namespace LoanService.Domain.Entities.Investment;


public class InvestmentAccount : BaseEntity
{

    private readonly List<InvestmentOperation> _operations = new();

    private InvestmentAccount() { }

    private InvestmentAccount(Guid? policyId,string? nationalCode,string? birthDate, InvestmentPlanType? planCode, string? traceId)
    {
        NationalCode = nationalCode;
        BirthDate = birthDate;
        PlanCode = planCode;
        LastTraceId = traceId;
        ProviderPolicyId = policyId;
        var op = InvestmentOperation.CreateAccount(policyId, traceId, "ایجاد حساب سرمایه‌گذاری");
        _operations.Add(op);
    }

    public Guid? ProviderPolicyId { get; private set; }

    public string? NationalCode { get; private set; } = default!;
    public string? BirthDate { get; private set; }
    public InvestmentPlanType? PlanCode { get; private set; } = default!; 

    public string? PostalCode { get; private set; }
    public string? Address { get; private set; }

    public InvestmentState State { get; private set; }

    // ----- خلاصه وضعیت مالی -----

    /// <summary>جمع کل سرمایه‌گذاری‌های انجام شده (بر اساس عملیات تأیید‌شده)</summary>
    public decimal TotalInvested { get; private set; }

    /// <summary>جمع کل برداشت‌های انجام‌شده</summary>
    public decimal TotalWithdrawn { get; private set; }

    /// <summary>ارزش روز دارایی (از سرویس revokable-amount / history به‌روزرسانی می‌شود)</summary>
    public decimal CurrentValue { get; private set; }

    /// <summary>مبلغ قابل برداشت (revokableAmount)؛ سقف مجاز برداشت مستقیم</summary>
    public decimal RevokableAmount { get; private set; }

    /// <summary>مبلغ وثیقه‌شده (در صورت استفاده وثیقه‌ای از این حساب)</summary>
    public decimal CollateralAmount { get; private set; }

    /// <summary>آخرین TraceId استفاده‌شده در یک عملیات موفق</summary>
    public string? LastTraceId { get; private set; }
    // log عملیات
    public IReadOnlyCollection<InvestmentOperation> Operations => _operations.AsReadOnly();

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    // ----- Factory -----

    /// <summary>
    /// ایجاد حساب سرمایه‌گذاری جدید بعد از دریافت PolicyID از سرویس کاریزما.
    /// این متد فقط منطق دامینی را انجام می‌دهد؛ فراخوانی سرویس خارجی در لایه Application/Infrastructure انجام می‌شود.
    /// </summary>
    public static InvestmentAccount CreateNew(
 
        Guid? policyId,
        string? nationalCode,
        string? birthDate,
        InvestmentPlanType? planCode,

        string? traceId)
    {


        if (string.IsNullOrEmpty(policyId.ToString()))
            throw new LogicException("PolicyId نامعتبر است.");

        if (string.IsNullOrWhiteSpace(nationalCode))
            throw new LogicException("کد ملی الزامی است.");

        if (string.IsNullOrWhiteSpace(planCode.ToString()))
            throw new LogicException("نوع طرح سرمایه‌گذاری الزامی است.");

        if (string.IsNullOrWhiteSpace(traceId))
            throw new LogicException(" برای ایجاد حساب  شناسه رهگیری الزامی است.");

        var acc = new InvestmentAccount(policyId,  nationalCode,  birthDate,  planCode,  traceId);

        return acc;
    }


    // ----- منطق کسب‌وکار: افزایش سرمایه (direct) -----

    /// <summary>
    /// ثبت افزایش سرمایه زمانی که پرداخت قبلاً انجام شده و فقط ثبت فیش/سند مالی است.
    /// مطابق سرویس /api/order/increase/direct
    /// </summary>
    public void RegisterIncreaseDirect(
        decimal amount,
        string traceId,
        DateTime receiptDate,
        string receiptNumber,
        string? description)
    {
        EnsureNotClosed();
        EnsurePositiveAmount(amount);
        EnsureTraceIdIsUnique(traceId);

        if (string.IsNullOrWhiteSpace(receiptNumber))
            throw new LogicException("شماره مرجع پرداخت (receiptNumber) الزامی است.");

        var op = InvestmentOperation.CreateIncreaseDirect(
            ProviderPolicyId,
            amount,
            traceId,
            receiptDate,
            receiptNumber,
            description);

        _operations.Add(op);

        TotalInvested += amount;
        RevokableAmount += amount; // چون این افزایش مستقیم است، فوراً قابل برداشت می‌شود
        LastTraceId = traceId;

        State = InvestmentState.Active;
        Touch();
    }

    // ----- منطق کسب‌وکار: افزایش سرمایه (آنلاین) -----

    /// <summary>
    /// شروع افزایش سرمایه با درگاه آنلاین (قبل از رفتن کاربر به درگاه).
    /// مطابق سرویس /api/order/increase → این مرحله قبل از پرداخت است.
    /// </summary>
    public void StartOnlineIncrease(
        decimal amount,
        string traceId,
        string? description)
    {
        EnsureNotClosed();
        EnsurePositiveAmount(amount);
        EnsureTraceIdIsUnique(traceId);

        var op = InvestmentOperation.CreateIncreaseOnlineRequested(
            ProviderPolicyId,
            amount,
            traceId,
            description);

        _operations.Add(op);
        LastTraceId = traceId;

        // هنوز پول واریز نشده، پس روی TotalInvested/RevokableAmount اثری ندارد.
        Touch();
    }

    /// <summary>
    /// بعد از بازگشت موفق از درگاه، افزایش سرمایه را نهایی می‌کند.
    /// (این متد را هنگام callback صدا می‌زنی)
    /// </summary>
    public void ConfirmOnlineIncreasePaid(
        string traceId,
        DateTime paidAt,
        string receiptNumber)
    {
        EnsureNotClosed();

        var op = _operations.FirstOrDefault(x => x.Type == InvestmentOperationType.IncreaseOnline && x.TraceId == traceId);

        if (op is null)
            throw new LogicException("عملیات افزایش آنلاین با این TraceId یافت نشد.");

        if (op.Status == InvestmentOrderState.Created)
            return; // idempotent

        if (string.IsNullOrWhiteSpace(receiptNumber))
            throw new LogicException("شماره مرجع پرداخت الزامی است.");

        op.MarkCompleted(receiptNumber, paidAt);

        TotalInvested += op.Amount;
        RevokableAmount += op.Amount;

        LastTraceId = traceId;
        State = InvestmentState.Active;
        Touch();
    }

    /// <summary>
    /// ثبت برداشت مستقیم بعد از این‌که مبلغ به کاربر پرداخت شده است.
    /// مطابق سرویس /api/order/decrease/direct
    /// </summary>
    public void RegisterDecreaseDirect(
        decimal amount,
        string traceId,
        DateTime receiptDate,
        string? description)
    {
        EnsureNotClosed();
        EnsurePositiveAmount(amount);
        EnsureTraceIdIsUnique(traceId);

     
        if (amount > RevokableAmount)
            throw new LogicException("مبلغ برداشت‌شده بیشتر از مبلغ قابل برداشت (revokableAmount) است.");

        var op = InvestmentOperation.CreateDecreaseDirect(ProviderPolicyId, amount,traceId,receiptDate,description);

        _operations.Add(op);
        TotalWithdrawn += amount;
        RevokableAmount -= amount;

        LastTraceId = traceId;


        if (RevokableAmount <= 0 && CurrentValue <= 0)
            State = InvestmentState.Closed;

        Touch();
    }


    /// <summary>
    /// به‌روزرسانی Snapshot ارزش دارایی و مبلغ قابل برداشت بر اساس خروجی سرویس
    /// /api/order/revokable-amount
    /// </summary>
    public void ApplyValuationSnapshot(decimal value, decimal revokableAmount, decimal collateralAmount)
    {
        if (value < 0 || revokableAmount < 0 || collateralAmount < 0)
            throw new LogicException("مقادیر ارزش/مانده قابل برداشت/وثیقه نمی‌توانند منفی باشند.");

        CurrentValue = value;
        RevokableAmount = revokableAmount;
        CollateralAmount = collateralAmount;

        Touch();
    }

    // ----- Helperهای دامینی -----

    private void EnsureNotClosed()
    {
        if (State == InvestmentState.Closed)
            throw new LogicException("روی حساب بسته‌شده نمی‌توان عملیات انجام داد.");
    }

    private static void EnsurePositiveAmount(decimal amount)
    {
        if (amount <= 0)
            throw new LogicException("مبلغ باید بزرگتر از صفر باشد.");
    }

    /// <summary>
    /// TraceId باید برای هر درخواست یکتا باشد (برای همین حساب).
    /// این متد جلوی دوباره‌کاری/ثبت تکراری را می‌گیرد.
    /// </summary>

    private void EnsureTraceIdIsUnique(string traceId)
    {
        if (string.IsNullOrWhiteSpace(traceId))
            throw new LogicException("TraceId الزامی است.");

        if (_operations.Any(x => x.TraceId == traceId))
            throw new LogicException("این TraceId قبلاً برای این حساب استفاده شده است.");
    }
}

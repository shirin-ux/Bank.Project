namespace Karizmah.Provider.Dtos;

/// <summary>
/// بدنه درخواست سرویس ایجاد PolicyID بدون پرداخت اولیه در کاریزما
/// </summary>
public sealed class KarizmahCreatePolicyWithoutInitialPaymentRequestDto
{
    public string? address { get; set; }
    public string birthDate { get; set; } = default!;
    public string nationalCode { get; set; } = default!;
    public string planTypeAliasName { get; set; } = default!;
    public long traceId { get; set; }
    public int age { get; set; } = 0;
    public int coefficient { get; set; } = 0;
    public string coverageAliasName { get; set; } = "0";
    public string? description { get; set; }
    public string postalCode { get; set; } = default!;
}


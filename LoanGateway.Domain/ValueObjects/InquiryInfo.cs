using LoanService.Domain.Enum;


namespace LoanService.Domain.ValueObjects
{
    /// <summary>
    /// نتیجه استعلام:
    /// </summary>
    /// <param name="Allowed"></param>
    /// <param name="MaxApprovedAmount"></param>
    /// <param name="Ics"></param>
    /// <param name="IcsGrade"></param>
    /// <param name="ExpireAt"></param>
    public sealed record InquiryInfo(
        bool? Allowed,
        decimal? MaxApprovedAmount,
        int? Ics,
        Grade? IcsGrade,
        DateTime? ExpireAt
    );
}

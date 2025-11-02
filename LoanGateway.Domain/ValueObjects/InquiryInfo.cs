using LoanService.Domain.Enum;


namespace LoanService.Domain.ValueObjects
{
    public sealed record InquiryInfo(
        bool? Allowed,
        decimal? MaxApprovedAmount,
        int? Ics,
        Grade? IcsGrade,
        DateTime? ExpireAt
    );
}

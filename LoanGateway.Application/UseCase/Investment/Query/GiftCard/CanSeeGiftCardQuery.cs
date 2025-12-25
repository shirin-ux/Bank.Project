using Common;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.GiftCard;

/// <summary>
/// Query برای بررسی اینکه آیا کاربر می‌تواند گزینه دریافت کارت هدیه را در فلوی سرمایه‌گذاری ببیند
/// </summary>
public class CanSeeGiftCardQuery : IRequest<Result<CanSeeGiftCardResult>>
{
    public Guid UserId { get; set; }
}

public class CanSeeGiftCardResult
{
    public bool CanSee { get; set; }
    public string Message { get; set; } = string.Empty;
}












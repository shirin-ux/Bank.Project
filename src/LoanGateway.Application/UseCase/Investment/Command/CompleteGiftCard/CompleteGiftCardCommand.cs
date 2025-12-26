using Common;
using LoanService.Domain.Enum.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.CompleteGiftCard;

public class CompleteGiftCardCommand : IRequest<Result<CompleteGiftCardResult>>
{
    // OrderId دیگر از فرانت نمی‌آید، از GiftCard موجود در دیتابیس گرفته می‌شود
    public Guid UserId { get; set; }
    public Guid ProviderPolicyId { get; set; }
    public InvestmentPlanType PlanType { get; set; }
    public long? TraceId { get; set; }
}

public class CompleteGiftCardResult
{
    public Guid GiftCardId { get; set; }
    public Guid InvestmentAccountId { get; set; }
    public Guid ProviderPolicyId { get; set; }
    public string Message { get; set; } = string.Empty;
}





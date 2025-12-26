using Common;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.GiftCard;
public class ReceiveGiftCardCommand : IRequest<Result<ReceiveGiftCardResult>>
{
    public Guid UserId { get; set; }
    //public string? GiftCardCode { get; set; }
    //public string? Description { get; set; }
}

public class ReceiveGiftCardResult
{
    public Guid GiftCardId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsPending { get; set; }
    public bool IsReceived { get; set; }
    public decimal? GiftCardAmountRial { get; set; }
    public decimal? EquivalentGrams { get; set; }
    public decimal? GramPrice { get; set; }
}


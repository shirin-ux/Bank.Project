using Common;
using LoanService.Domain.IRepository.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.GiftCard;

public class CanSeeGiftCardQueryHandler(IGiftCardRepository giftCardRepository) 
    : IRequestHandler<CanSeeGiftCardQuery, Result<CanSeeGiftCardResult>>
{
    private readonly IGiftCardRepository _giftCardRepository = giftCardRepository;

    public async Task<Result<CanSeeGiftCardResult>> Handle(CanSeeGiftCardQuery request, CancellationToken cancellationToken)
    {
        var canSee = await _giftCardRepository.IsUserEligibleForGiftCardAsync(request.UserId, cancellationToken);

        var result = new CanSeeGiftCardResult
        {
            CanSee = canSee,
            Message = canSee ? "شما می‌توانید کارت هدیه را دریافت کنید" : "شما مجاز به دریافت کارت هدیه نیستید"
        };

        return Result<CanSeeGiftCardResult>.Success(result);
    }
}


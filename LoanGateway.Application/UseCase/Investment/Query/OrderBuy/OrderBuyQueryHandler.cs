using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum.Investment;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.OrderBuy;


public class GetOrderStatusQueryHandler
    : IRequestHandler<GetOrderStatusQuery, Result<GetOrderStatusResultDto>>
{
    private readonly IInvestmentProvider _karizmahProvider;

    public GetOrderStatusQueryHandler(IInvestmentProvider karizmahProvider)
    {
        _karizmahProvider = karizmahProvider;
    }

    public async Task<Result<GetOrderStatusResultDto>> Handle(
        GetOrderStatusQuery request,
        CancellationToken cancellationToken)
    {

        var order = await _karizmahProvider.GetOrderBuyByIdAsync(request.Id, cancellationToken);

        var sellStatus = order.Status;
        var buyStatus = order.UliStatus;

        var result = new GetOrderStatusResultDto();

        if (sellStatus == statusType.FinalStatus && !buyStatus)
        {
            result.UliStatus = true;
            result.Status = statusType.FinalStatus;
              return Result<GetOrderStatusResultDto>.Success(result);
        }

      
        if (sellStatus == statusType.ProcessingStatus && buyStatus)
        {
            result.UliStatus = false;
            result.Status = statusType.ProcessingStatus;
            return Result<GetOrderStatusResultDto>.Success(result);
        }


        if (sellStatus == statusType.CancelledStatus && !buyStatus)
        {
            result.UliStatus = false;
            result.Status = statusType.CancelledStatus;
            return Result<GetOrderStatusResultDto>.Success(result);
        }


        result.UliStatus = false;
        result.Status = 0;

        return  Result<GetOrderStatusResultDto>.Success(result);
    }
}
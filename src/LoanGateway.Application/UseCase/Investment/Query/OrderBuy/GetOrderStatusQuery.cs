using Common;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Query.OrderBuy
{
    public class GetOrderStatusQuery : IRequest<Result<GetOrderStatusResultDto>>
    {
        public Guid Id { get; set; }
    }
}

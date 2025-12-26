using Common;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.CompletePayment
{
    public sealed record CompleteSadadPaymentCommand(SadadCallbackDto Callback) : IRequest<Result<CompleteSadadPaymentResultDto>>;

}

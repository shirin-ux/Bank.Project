using Common;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.User;


public class ReceiveZipCodeCommandHandler : IRequestHandler<ReceiveZipCodeCommand, Result<ReceiveZipCodeResult>>
{

    public async Task<Result<ReceiveZipCodeResult>> Handle(ReceiveZipCodeCommand request, CancellationToken cancellationToken)
    {

        var res = new ReceiveZipCodeResult
        {
            PostalCode = request.PostalCode,
            Message = "کدپستی معتبر دریافت شد"
        };
        return Result<ReceiveZipCodeResult>.Success(res);
    }
}

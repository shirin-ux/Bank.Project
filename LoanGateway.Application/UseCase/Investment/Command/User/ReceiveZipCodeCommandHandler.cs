using Common;
using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.User;


public class ReceiveZipCodeCommandHandler( IUserReadService userReadService) : IRequestHandler<ReceiveZipCodeCommand, Result<ReceiveZipCodeResult>>
{

    private readonly IUserReadService _userReadService = userReadService;
    public async Task<Result<ReceiveZipCodeResult>> Handle(ReceiveZipCodeCommand request, CancellationToken cancellationToken)
    {
       var res=await _userReadService.UpdateUserAsync(request.UserId,request.PostalCode, cancellationToken);

        var result = new ReceiveZipCodeResult
        {
            PostalCode = request.PostalCode,
            Message = "کدپستی معتبر دریافت شد"
        };
        return Result<ReceiveZipCodeResult>.Success(result);
    }
}

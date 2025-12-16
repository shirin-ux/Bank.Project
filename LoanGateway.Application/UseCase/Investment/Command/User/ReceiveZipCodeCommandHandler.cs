using Common;
using LoanService.Application.Contracts;
using MediatR;

namespace LoanService.Application.UseCase.Investment.Command.User;


public class ReceiveZipCodeCommandHandler(IUserApiClient userApi) : IRequestHandler<ReceiveZipCodeCommand, Result<ReceiveZipCodeResult>>
{
    private readonly IUserApiClient _userApi= userApi;
    public async Task<Result<ReceiveZipCodeResult>> Handle(ReceiveZipCodeCommand request, CancellationToken cancellationToken)
    {
       var res=await _userApi.UpdateUserAsync(new UserIdRequest { PostalCode = request.PostalCode, UserId = request.UserId }, cancellationToken);

        var result = new ReceiveZipCodeResult
        {
            PostalCode = request.PostalCode,
            Message = "کدپستی معتبر دریافت شد"
        };
        return Result<ReceiveZipCodeResult>.Success(result);
    }
}

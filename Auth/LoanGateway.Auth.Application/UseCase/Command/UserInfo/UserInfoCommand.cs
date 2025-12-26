using Common;
using LoanGateway.Auth.Application.UseCase.Command.CompleteProfile;
using MediatR;

namespace LoanGateway.Auth.Application.UseCase.Command.UserInfo;

public sealed class UserInfoCommand : IRequest<Result<UserInfoResultDto>>
{
    public string NationalCode { get; set; }
}




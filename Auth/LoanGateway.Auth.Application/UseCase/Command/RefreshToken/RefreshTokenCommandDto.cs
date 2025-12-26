using Common;
using MediatR;


namespace LoanGateway.Auth.Application.UseCase.Command.RefreshToken
{
    public sealed class RefreshTokenCommandDto : IRequest<Result<RefreshTokenResultDto>>
    {

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}

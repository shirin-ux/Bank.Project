using Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.Logout;

public sealed class LogoutCommandDto : IRequest<Result>
{
    public LogoutCommandDto(string refreshToken)
    {
        RefreshToken = refreshToken;
    }

    public string RefreshToken { get; }
}
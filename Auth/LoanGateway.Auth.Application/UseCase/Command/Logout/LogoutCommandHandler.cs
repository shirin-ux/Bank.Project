using Common;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.Logout
{

    public sealed class LogoutCommandHandler
        : IRequestHandler<LogoutCommandDto, Result>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;


        public LogoutCommandHandler(
            IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<Result> Handle(LogoutCommandDto request, CancellationToken ct)
        {
            var hash = DateExtensions.Hash(request.RefreshToken);
            var nowUtc = DateTime.UtcNow;

            await _refreshTokenRepository.RevokeByHashAsync(
                hash,
                nowUtc,
                "UserLogout",
                ct);

            return Result.Success();
        }
    }
}

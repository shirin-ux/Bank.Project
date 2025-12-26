using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.RefreshToken
{
   public class RefreshTokenResultDto
    {
        public string AccessToken { get; init; } = default!;
        public DateTime AccessTokenExpiresAtUtc { get; init; }

        public string RefreshToken { get; init; } = default!;
        public DateTime RefreshTokenExpiresAtUtc { get; init; }
    }
}

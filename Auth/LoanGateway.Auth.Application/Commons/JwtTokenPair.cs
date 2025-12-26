using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.Commons
{

        public sealed class JwtTokenPair
        {
            public string AccessToken { get; init; } = default!;
            public DateTime AccessTokenExpiresAtUtc { get; init; }

            public string RefreshToken { get; init; } = default!;
            public DateTime RefreshTokenExpiresAtUtc { get; init; }

            public Guid JwtId { get; init; }
        }
    

}

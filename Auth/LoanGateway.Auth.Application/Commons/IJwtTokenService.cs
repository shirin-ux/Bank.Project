using LoanGateway.Auth.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.Commons
{
    public interface IJwtTokenService
    {
        Task<JwtTokenPair> GenerateTokensAsync(User user, CancellationToken ct);

        Task<JwtTokenPair> GetTokenPairAsync(Guid refreshTokenId, CancellationToken ct);

    }
   
}

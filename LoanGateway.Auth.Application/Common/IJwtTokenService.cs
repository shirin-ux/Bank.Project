using LoanGateway.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.Common
{
    public interface IJwtTokenService
    {
        Task<JwtTokenPair> GenerateTokensAsync(User user, CancellationToken ct);
    }
}

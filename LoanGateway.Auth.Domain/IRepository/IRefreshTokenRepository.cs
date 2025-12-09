using LoanGateway.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.IRepository
{
    public interface IRefreshTokenRepository
    {
        Task InsertAsync(RefreshToken token, CancellationToken ct);
    }
}

using LoanGateway.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.IRepository
{
    public interface IUserReadRepository
    {
        Task<bool> ExistsByMobileOrNationalCodeAsync( string mobileNumber,string nationalCode,CancellationToken ct);
        Task<User?> GetByMobileAsync(string mobileNumber, CancellationToken ct);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<User> InsertAsync(User user, CancellationToken ct);
        Task UpdateProfileAsync(User user, CancellationToken ct);

    }
}

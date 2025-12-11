using LoanGateway.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.IRepository
{
    public interface IUserProfileRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<bool> ExistsByNationalCodeAsync( string nationalCode, Guid? excludeUserId,CancellationToken ct);

        Task UpdateProfileAfterKycAsync(User user, CancellationToken ct);
    }
}

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
    }
}

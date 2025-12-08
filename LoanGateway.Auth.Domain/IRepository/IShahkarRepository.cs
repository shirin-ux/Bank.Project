using LoanGateway.Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.IRepository
{
    public interface IShahkarRepository
    {
        Task InsertAsync(VerfiyMobileOwnerInquiry log, CancellationToken cancellationToken = default);
    }
}

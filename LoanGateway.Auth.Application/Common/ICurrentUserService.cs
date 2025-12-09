using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.Common
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string? MobileNumber { get; }
    }
}

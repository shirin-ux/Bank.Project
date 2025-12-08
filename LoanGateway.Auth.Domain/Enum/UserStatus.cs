using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.Enum
{
    public enum UserStatus : byte
    {
        Inactive = 0,
        Active = 1,
        Blocked = 2
    }
}

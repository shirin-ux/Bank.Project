using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.Enum
{
    public enum OtpPurpose : byte
    {
        Register = 1,
        Login = 2,
        ResetPassword = 3
    }
}

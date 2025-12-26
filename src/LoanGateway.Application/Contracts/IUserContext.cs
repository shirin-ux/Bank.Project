using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts
{
    public interface IUserContext
    {
        Guid UserId { get; }
        string? NationalCode { get; }
        bool IsAuthenticated { get; }
    }
}

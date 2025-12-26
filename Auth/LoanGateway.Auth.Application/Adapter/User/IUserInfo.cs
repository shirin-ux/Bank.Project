using Common;
using LoanGateway.Auth.Application.UseCase.Command.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.Adapter.User
{
    public interface IUserInfo
    {
        Task<Result<UserInfoResultDto>> GetUserInfo(CancellationToken ct);
    }
}

using Common;
using LoanGateway.Auth.Application.Adapter.User;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Application.UseCase.Command.UserInfo;
using LoanGateway.Auth.Domain.IRepository;

namespace LoanGateway.Auth.Infrastructure.Services
{
    public class UserInfo(IUserReadRepository repo, ICurrentUserService currentUser) : IUserInfo
    {
        private readonly ICurrentUserService _currentUser = currentUser;
        public IUserReadRepository _repo = repo;
        public async Task<Result<UserInfoResultDto>> GetUserInfo(CancellationToken ct)
        {
            var userId = _currentUser.UserId;
            var user = await _repo.GetByIdAsync(userId, ct);
            var res=new UserInfoResultDto
            {
              MobileNumber=user.MobileNumber,
              BirthDate=user.BirthDate,
              NationalCode=user.NationalCode,
               PostalCode=user.PostalCode,
               Address=user.Address,
               ShenasnamehNumber=user.ShenasnamehNumber,
               ShenasnameSeri=user.ShenasnameSeri,
               ShenasnameSerial=user.ShenasnameSerial,
               UserId=user.Id
            };
            return Result<UserInfoResultDto>.Success(res);
        }
    }
}

using LoanGateway.Auth.Application.Commons;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace LoanGateway.Auth.Infrastructure.Communication
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User
                    ?? throw new UnauthorizedAccessException();

                var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (sub is null || !Guid.TryParse(sub, out var id))
                    throw new UnauthorizedAccessException("شناسه کاربر در توکن یافت نشد.");

                return id;
            }
        }

        public string? MobileNumber
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.FindFirst(ClaimTypes.MobilePhone)?.Value
                       ?? user?.FindFirst(JwtRegisteredClaimNames.PhoneNumber)?.Value;
            }
        }
    }
}

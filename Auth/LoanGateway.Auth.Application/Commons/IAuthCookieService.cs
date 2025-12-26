using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.Commons
{
    public interface IAuthCookieService
    {
        void SetRefreshToken(HttpContext httpContext, string token, DateTime expiresAtUtc);
        void ClearRefreshToken(HttpContext httpContext);
        string? GetRefreshToken(HttpContext httpContext);
    }
}

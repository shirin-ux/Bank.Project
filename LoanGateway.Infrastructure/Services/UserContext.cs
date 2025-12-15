using LoanService.Application.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Infrastructure.Contracts;

public sealed class UserContext : IUserContext
{
    public Guid UserId { get; }
    public string? NationalCode { get; }
    public bool IsAuthenticated { get; }

    public UserContext(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;

        IsAuthenticated = user?.Identity?.IsAuthenticated == true;

        if (!IsAuthenticated)
            return;

        UserId = Guid.Parse(user!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        NationalCode = user.FindFirst("national_code")?.Value;
    }
}

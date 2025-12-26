using LoanGateway.Auth.Application.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using LoanGateway.Auth.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;

namespace LoanGateway.Auth.Infrastructure.Communication;

public class AuthCookieService(IOptions<AuthCookieOptions> options,
    IDataProtectionProvider dataProtectionProvider, 
    ILogger<AuthCookieService> logger) : IAuthCookieService
{
    private readonly IOptions<AuthCookieOptions> _options= options;
    private readonly ILogger<AuthCookieService> _logger= logger;
    private readonly IDataProtectionProvider _dataProtectionProvider= dataProtectionProvider;

        public void SetRefreshToken(
            HttpContext httpContext,
            string token,
            DateTime expiresAtUtc)
        {
            try
            {
                var options = _options;
                var isHttps = httpContext.Request.IsHttps;

                // Secure flag باید فقط زمانی true باشد که درخواست واقعاً HTTPS است
                // در غیر این صورت مرورگر کوکی را برنمی‌گرداند
                var secureFlag = isHttps && options.Value.Secure;

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = secureFlag,
                    SameSite = options.Value.SameSite?.ToLowerInvariant() switch
                    {
                        "lax" => SameSiteMode.Lax,
                        "strict" => SameSiteMode.Strict,
                        _ => SameSiteMode.None
                    },
                    Path = string.IsNullOrWhiteSpace(options.Value.Path) ? "/" : options.Value.Path,
                    Expires = expiresAtUtc,
                    IsEssential = true,
                    Domain = null
                };

                var protector = _dataProtectionProvider.CreateProtector("RefreshToken");
                var encryptedToken = protector.Protect(token);

                httpContext.Response.Cookies.Delete(
                    _options.Value.RefreshTokenCookieName,
                    new CookieOptions
                    {
                        Path = cookieOptions.Path,
                        Secure = cookieOptions.Secure,
                        SameSite = cookieOptions.SameSite,
                        Domain = cookieOptions.Domain
                    });

                httpContext.Response.Cookies.Append(
                    _options.Value.RefreshTokenCookieName,
                    encryptedToken,
                    cookieOptions);

                _logger.LogInformation(
                    "Refresh token cookie set. IsHttps: {IsHttps}, Secure: {Secure}, SameSite: {SameSite}, Host: {Host}, CookieName: {CookieName}",
                    isHttps, cookieOptions.Secure, cookieOptions.SameSite,
                    httpContext.Request.Host, _options.Value.RefreshTokenCookieName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting refresh token cookie");
                throw;
            }
        }

        //public void SetRefreshToken(
        //    HttpContext httpContext,
        //    string token,
        //    DateTime expiresAtUtc)
        //{
        //    try
        //    {
        //        var options = _options;

        //        // بررسی محیط اجرا برای Secure flag
        //        var isProduction = httpContext.Request.IsHttps ||
        //                           !httpContext.Request.Host.Host.Contains("localhost");

        //        var cookieOptions = new CookieOptions
        //        {
        //            HttpOnly = true, // جلوگیری از دسترسی JavaScript
        //            Secure = options.Value.Secure || isProduction, // در production باید true باشد
        //            SameSite = options.Value.SameSite?.ToLowerInvariant() switch
        //            {
        //                "lax" => SameSiteMode.Lax,
        //                "strict" => SameSiteMode.Strict,
        //                _ => SameSiteMode.None // برای cross-site
        //            },
        //            Path = string.IsNullOrWhiteSpace(options.Value.Path) ? "/" : options.Value.Path,
        //            Expires = expiresAtUtc,
        //            IsEssential = true, // برای GDPR compliance
        //            Domain = null // فقط برای domain فعلی
        //        };

        //        httpContext.Response.Cookies.Delete(
        //            options.Value.RefreshTokenCookieName,
        //            new CookieOptions
        //            {
        //                Path = cookieOptions.Path,
        //                Secure = cookieOptions.Secure,
        //                SameSite = cookieOptions.SameSite,
        //                Domain = cookieOptions.Domain
        //            });

        //        // تنظیم cookie جدید
        //        httpContext.Response.Cookies.Append(
        //            options.Value.RefreshTokenCookieName,
        //            token,
        //            cookieOptions);

        //        _logger.LogDebug("Refresh token cookie set. Secure: {Secure}, SameSite: {SameSite}",
        //            cookieOptions.Secure, cookieOptions.SameSite);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}
        public string? GetRefreshToken(HttpContext httpContext)
    {
        var cookieName = _options.Value.RefreshTokenCookieName;
        var encryptedToken = httpContext.Request.Cookies[cookieName];
        
        if (string.IsNullOrWhiteSpace(encryptedToken))
        {
            _logger.LogWarning(
                "Refresh token cookie not found. CookieName: {CookieName}, IsHttps: {IsHttps}, Host: {Host}, AllCookies: {Cookies}",
                cookieName, 
                httpContext.Request.IsHttps,
                httpContext.Request.Host,
                string.Join(", ", httpContext.Request.Cookies.Keys));
            return null;
        }

        try
        {
            var protector = _dataProtectionProvider.CreateProtector("RefreshToken");
            var decryptedToken = protector.Unprotect(encryptedToken);
            _logger.LogDebug("Refresh token successfully decrypted from cookie");
            return decryptedToken;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, 
                "Failed to decrypt refresh token. CookieName: {CookieName}, IsHttps: {IsHttps}",
                cookieName, httpContext.Request.IsHttps);
            return null;
        }
    }
    public void ClearRefreshToken(HttpContext httpContext)
    {
        var options = _options.Value;
        var isHttps = httpContext.Request.IsHttps;
        var secureFlag = isHttps && options.Secure;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secureFlag,
            Path = string.IsNullOrWhiteSpace(options.Path) ? "/" : options.Path,
            SameSite = options.SameSite?.ToLowerInvariant() switch
            {
                "lax" => SameSiteMode.Lax,
                "strict" => SameSiteMode.Strict,
                _ => SameSiteMode.None
            },
            Expires = DateTimeOffset.UtcNow.AddDays(-1) // حذف با انقضا در گذشته
        };

        httpContext.Response.Cookies.Delete(options.RefreshTokenCookieName, cookieOptions);

        _logger.LogDebug("Refresh token cookie cleared. IsHttps: {IsHttps}, Secure: {Secure}", 
            isHttps, secureFlag);
    }
}
//private readonly IOptions<AuthCookieOptions> _options = options;
//public void SetRefreshToken(
//HttpContext httpContext,
//string token,
//DateTime expiresAtUtc)
//{
//    var options = _options.Value;

//    var cookieOptions = new CookieOptions
//    {
//        HttpOnly = true,
//        Secure = options.Secure,              // 🔴 در prod باید true
//        SameSite = options.SameSite?.ToLowerInvariant() switch
//        {
//            "lax" => SameSiteMode.Lax,
//            "strict" => SameSiteMode.Strict,
//            _ => SameSiteMode.None              // 🔴 برای cross-site
//        },
//        Path = string.IsNullOrWhiteSpace(options.Path)
//            ? "/"                               // 🔴 بسیار مهم
//            : options.Path,
//        Expires = expiresAtUtc,
//        IsEssential = true                     // 🔵 توصیه‌شده
//    };

//    httpContext.Response.Cookies.Delete(
//        options.RefreshTokenCookieName,
//        new CookieOptions
//        {
//            Path = cookieOptions.Path,
//            Secure = cookieOptions.Secure,
//            SameSite = cookieOptions.SameSite
//        });

//    httpContext.Response.Cookies.Append(
//        options.RefreshTokenCookieName,
//        token,
//        cookieOptions);
//}


//public void ClearRefreshToken(HttpContext httpContext)
//{
//    var cookieOptions = new CookieOptions
//    {
//        HttpOnly = true,
//        Secure = _options.Value.Secure,
//        Path = _options.Value.Path,
//        SameSite = SameSiteMode.None
//    };

//    httpContext.Response.Cookies.Delete(_options.Value.RefreshTokenCookieName, cookieOptions);

//}


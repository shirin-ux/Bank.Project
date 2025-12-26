
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanGateway.Auth.Api.Middlewares;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
    ILogger<RateLimitMiddleware> logger,
        IOptions<RateLimitOptions> options)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldRateLimit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var clientIp = GetClientIpAddress(context);
        var endpoint = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var cacheKey = $"rate_limit:{endpoint}:{clientIp}";

        var requestCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.WindowSeconds);
            entry.Priority = CacheItemPriority.Normal;
            return 0;
        })!;

        if (requestCount >= _options.MaxRequests)
        {
            _logger.LogWarning(
                "Rate limit exceeded for IP {IpAddress} on endpoint {Endpoint}. Requests: {RequestCount}/{MaxRequests}",
                clientIp, endpoint, requestCount, _options.MaxRequests);

            var statusCode = StatusCodes.Status429TooManyRequests;
            var retryAfter = _options.WindowSeconds;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Headers["Retry-After"] = retryAfter.ToString();

            var errorResponse = new
            {
                error = "تعداد درخواست‌ها بیش از حد مجاز است. لطفاً بعداً تلاش کنید.",
                code = statusCode,
                details = new
                {
                    retryAfterSeconds = retryAfter
                },
                traceId = context.TraceIdentifier
            };

            var json = System.Text.Json.JsonSerializer.Serialize(errorResponse,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            await context.Response.WriteAsync(json);
            return;
        }

        _cache.Set(cacheKey, requestCount + 1, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.WindowSeconds)
        });

        await _next(context);
    }

    private static bool ShouldRateLimit(PathString path)
    {
        var rateLimitPaths = new[]
        {
            "/api/auth/create-otp",
            "/api/auth/verify-otp",
            "/api/auth/refresh-token"
        };

        return rateLimitPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // بررسی X-Forwarded-For برای proxy/load balancer
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ipAddress = forwardedFor.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "unknown")
                return ipAddress;
        }

        // بررسی X-Real-IP
        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var ipAddress = realIp.ToString().Trim();
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress != "unknown")
                return ipAddress;
        }

        // استفاده از RemoteIpAddress به عنوان آخرین گزینه
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
using FluentValidation;
using LoanGateway.Auth.Domain.Exceptions;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

namespace LoanGateway.Auth.Api.Middlewares
{
    public class GlobalExceptionMiddleware(RequestDelegate next, IWebHostEnvironment environment, ILogger<GlobalExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;
        private readonly IWebHostEnvironment _environment = environment;
        //    public async Task InvokeAsync(HttpContext context)
        //    {
        //        try
        //        {
        //            await _next(context);
        //        }
        //        catch (Exception ex)
        //        {

        //            await HandleExceptionAsync(context, ex);
        //        }
        //    }

        //    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        //    {
        //        _logger.LogError(ex, "Unhandled exception caught by GlobalExceptionMiddleware");

        //        int statusCode;
        //        string message;
        //        object? details = null;
        //        switch (ex)
        //        {
        //            case ValidationException validationException:
        //                statusCode = StatusCodes.Status400BadRequest;
        //                message = "خطا در اعتبار سنجی ورودی";
        //                details = validationException.Errors.Select(e => new
        //                {
        //                    field = e.PropertyName,
        //                    erro = e.ErrorMessage
        //                });
        //                break;

        //            case BaseAppException appEx:
        //                statusCode = (int)appEx.StatusCode;
        //                message = appEx.Message;
        //                details = appEx.Details;
        //                break;

        //            case UnauthorizedAccessException unauthEx:
        //                statusCode = StatusCodes.Status401Unauthorized;
        //                message = string.IsNullOrWhiteSpace(unauthEx.Message)
        //                    ? "دسترسی غیرمجاز."
        //                    : unauthEx.Message;
        //                break;



        //            case SqlException sqlEx when sqlEx.Number is 2627 or 2601:
        //                statusCode = StatusCodes.Status409Conflict;
        //                message = "داده‌ی تکراری ثبت شده است. امکان انجام این عملیات وجود ندارد.";
        //                details = new { sqlErrorNumber = sqlEx.Number };
        //                break;

        //            case SqlException sqlEx when sqlEx.Number == 547:
        //                statusCode = StatusCodes.Status409Conflict;
        //                message = "به علت وابستگی داده، این عملیات قابل انجام نیست.";
        //                details = new { sqlErrorNumber = sqlEx.Number };
        //                break;
        //            case SqlException sqlEx when sqlEx.Number == 1205:
        //                statusCode = StatusCodes.Status503ServiceUnavailable;
        //                message = "به علت ترافیک بالا (deadlock) عملیات انجام نشد. لطفاً دوباره تلاش کنید.";
        //                details = new { sqlErrorNumber = sqlEx.Number };
        //                break;
        //            case SqlException sqlEx:
        //                statusCode = StatusCodes.Status500InternalServerError;
        //                message = "خطای پایگاه داده رخ داد.";
        //                details = new { sqlErrorNumber = sqlEx.Number };
        //                break;
        //            case HttpRequestException httpEx:
        //                statusCode = StatusCodes.Status503ServiceUnavailable;
        //                message = "ارتباط با سرویس خارجی با مشکل مواجه شد.";
        //                details = new
        //                {
        //                    httpEx.Message,
        //                    StatusCode = httpEx.StatusCode
        //                };
        //                break;
        //            case TaskCanceledException:
        //            case OperationCanceledException:
        //            case TimeoutException:
        //                statusCode = StatusCodes.Status504GatewayTimeout;
        //                message = "زمان انجام عملیات به پایان رسید. لطفاً دوباره تلاش کنید.";
        //                break;
        //            default:
        //                statusCode = StatusCodes.Status500InternalServerError;
        //                message = "خطای غیرمنتظره‌ای در سرور رخ داد.";
        //                break;
        //        }
        //        var errorResponse = new
        //        {
        //            error = message,
        //            code = statusCode,
        //            details,

        //            traceId = context.TraceIdentifier
        //        };
        //        //context.Response.Headers["X-Trace-Id"] = traceId;
        //        //context.Response.Headers["X-Correlation-Id"] = correlationId;
        //        context.Response.Clear();
        //        context.Response.StatusCode = statusCode;
        //        context.Response.ContentType = "application/json; charset=utf-8";

        //        var json = JsonSerializer.Serialize(errorResponse);
        //        await context.Response.WriteAsync(json);
        //    }


        //    private static string? GetCorrelationId(HttpContext context)
        //    {
        //        // با استاندارد تیم خودت یکی کن: X-Correlation-Id یا x-request-id
        //        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid))
        //            return cid.ToString();

        //        if (context.Request.Headers.TryGetValue("x-request-id", out var rid) && !string.IsNullOrWhiteSpace(rid))
        //            return rid.ToString();

        //        return null;
        //    }
        //}



        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception caught by GlobalExceptionMiddleware. TraceId: {TraceId}",
                context.TraceIdentifier);

            int statusCode;
            string message;
            object? details = null;
            bool includeStack = false;

            var isDevelopment = _environment.IsDevelopment();

            switch (ex)
            {
                case ValidationException validationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "خطا در اعتبار سنجی ورودی";
                    details = validationException.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage // تصحیح: erro -> error
                    }).ToList();
                    break;

                case BaseAppException appEx:
                    statusCode = (int)appEx.StatusCode;
                    message = appEx.Message;
                    //details = SanitizeDetails(appEx.Details, !isDevelopment);
                    break;

                case UnauthorizedAccessException unauthEx:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = string.IsNullOrWhiteSpace(unauthEx.Message)
                        ? "دسترسی غیرمجاز"
                        : "دسترسی غیرمجاز"; // عدم افشای پیام اصلی در production
                                            // جزئیات بیشتر لاگ می‌شود اما به کلاینت ارسال نمی‌شود
                    _logger.LogWarning("Unauthorized access attempt. Message: {Message}", unauthEx.Message);
                    break;

                case SqlException sqlEx when sqlEx.Number is 2627 or 2601:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "این رکورد قبلاً ثبت شده است. امکان انجام این عملیات وجود ندارد.";
                    // عدم افشای شماره خطای SQL در production
                    if (!isDevelopment)
                    {
                        details = new { sqlErrorNumber = sqlEx.Number };
                    }
                    break;

                case SqlException sqlEx when sqlEx.Number == 547:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "به علت وابستگی داده‌ای، این عملیات قابل انجام نیست.";
                    if (!isDevelopment)
                    {
                        details = new { sqlErrorNumber = sqlEx.Number };
                    }
                    break;

                case SqlException sqlEx when sqlEx.Number == 1205:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    message = "سیستم در حال حاضر شلوغ است. لطفاً دوباره تلاش کنید.";
                    if (!isDevelopment)
                    {
                        details = new { sqlErrorNumber = sqlEx.Number };
                    }
                    break;

                case SqlException sqlEx:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "خطایی در پردازش درخواست رخ داد.";
                    // لاگ کردن جزئیات اما عدم ارسال به کلاینت
                    _logger.LogError(sqlEx,
                        "SQL Error {ErrorNumber} occurred. TraceId: {TraceId}",
                        sqlEx.Number, context.TraceIdentifier);
                    if (!isDevelopment)
                    {
                        details = new { sqlErrorNumber = sqlEx.Number };
                    }
                    break;

                case HttpRequestException httpEx:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    message = "ارتباط با سرویس خارجی با مشکل مواجه شد.";
                    // لاگ کردن جزئیات اما عدم ارسال به کلاینت
                    _logger.LogError(httpEx,
                        "External service error. StatusCode: {StatusCode}, TraceId: {TraceId}",
                        httpEx.Data["StatusCode"], context.TraceIdentifier);
                    // عدم افشای جزئیات خطا به کلاینت
                    break;

                case TaskCanceledException:
                case OperationCanceledException:
                case TimeoutException:
                    statusCode = StatusCodes.Status504GatewayTimeout;
                    message = "زمان انجام عملیات به پایان رسید. لطفاً دوباره تلاش کنید.";
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "خطای غیرمنتظره‌ای در سرور رخ داد.";
                    includeStack = isDevelopment;
                    break;
            }

            var errorResponse = new
            {
                error = message,
                code = statusCode,
                details,
                traceId = context.TraceIdentifier,
                stackTrace = includeStack ? ex.StackTrace : null
            };

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = isDevelopment
            };

            var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(json);
        }

        private object? SanitizeDetails(object? details, bool isDevelopment)
        {
            if (details == null)
                return null;

            // در production، جزئیات حساس را حذف کنید
            if (!isDevelopment)
            {
                // می‌توانید logic پیچیده‌تری برای حذف فیلدهای حساس اضافه کنید
                // مثلاً: password, token, secret, key و...

                // اگر details یک dictionary است، فیلدهای حساس را حذف کنید
                if (details is Dictionary<string, object> dict)
                {
                    var sensitiveKeys = new[] { "password", "token", "secret", "key", "connectionString" };
                    var sanitized = new Dictionary<string, object>();

                    foreach (var kvp in dict)
                    {
                        if (!sensitiveKeys.Any(sk => kvp.Key.Contains(sk, StringComparison.OrdinalIgnoreCase)))
                        {
                            sanitized[kvp.Key] = kvp.Value;
                        }
                    }

                    return sanitized.Count > 0 ? sanitized : null;
                }
            }

            return details;
        }
    }
}



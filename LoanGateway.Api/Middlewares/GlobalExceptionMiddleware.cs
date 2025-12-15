using FluentValidation;
using LoanGateway.Auth.Domain.Exceptions;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace LoanService.Api.Middlewares
{
    public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private ILogger<GlobalExceptionMiddleware> _logger = logger;

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
            _logger.LogError(ex, "Unhandled exception caught by GlobalExceptionMiddleware");

            int statusCode;
            string message;
            object? details = null;
            switch (ex)
            {
                case ValidationException validationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "خطا در اعتبار سنجی ورودی";
                    details = validationException.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        erro = e.ErrorMessage
                    });
                    break;

                case BaseAppException appEx:
                    statusCode = (int)appEx.StatusCode;
                    message = appEx.Message;
                    details = appEx.Details;
                    break;

                case UnauthorizedAccessException unauthEx:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = string.IsNullOrWhiteSpace(unauthEx.Message)
                        ? "دسترسی غیرمجاز."
                        : unauthEx.Message;
                    break;



                case SqlException sqlEx when sqlEx.Number is 2627 or 2601:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "داده‌ی تکراری ثبت شده است. امکان انجام این عملیات وجود ندارد.";
                    details = new { sqlErrorNumber = sqlEx.Number };
                    break;

                case SqlException sqlEx when sqlEx.Number == 547:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "به علت وابستگی داده، این عملیات قابل انجام نیست.";
                    details = new { sqlErrorNumber = sqlEx.Number };
                    break;
                case SqlException sqlEx when sqlEx.Number == 1205:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    message = "به علت ترافیک بالا (deadlock) عملیات انجام نشد. لطفاً دوباره تلاش کنید.";
                    details = new { sqlErrorNumber = sqlEx.Number };
                    break;
                case SqlException sqlEx:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "خطای پایگاه داده رخ داد.";
                    details = new { sqlErrorNumber = sqlEx.Number };
                    break;
                case HttpRequestException httpEx:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    message = "ارتباط با سرویس خارجی با مشکل مواجه شد.";
                    details = new
                    {
                        httpEx.Message,
                        StatusCode = httpEx.StatusCode
                    };
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
                    break;
            }
            var errorResponse = new
            {
                error = message,
                code = statusCode,
                details,

                traceId = context.TraceIdentifier
            };
            //context.Response.Headers["X-Trace-Id"] = traceId;
            //context.Response.Headers["X-Correlation-Id"] = correlationId;
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }


        private static string? GetCorrelationId(HttpContext context)
        {
            // با استاندارد تیم خودت یکی کن: X-Correlation-Id یا x-request-id
            if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid))
                return cid.ToString();

            if (context.Request.Headers.TryGetValue("x-request-id", out var rid) && !string.IsNullOrWhiteSpace(rid))
                return rid.ToString();

            return null;
        }
    }
}


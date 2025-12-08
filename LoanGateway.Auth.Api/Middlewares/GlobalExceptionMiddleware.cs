using FluentValidation;
using LoanGateway.Auth.Application.Exceptions;
using LoanService.Domain.Exceptions;
using System.Text.Json;

namespace LoanGateway.Auth.Api.Middlewares
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

                case NotFoundException notFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    message = notFoundEx.Message;
                    break;
                case LogicException logicEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = logicEx.Message;
                    break;
                case ExternalServiceException extEx:
                    statusCode = extEx.ExternalStatusCode;
                    message =extEx.Message;
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

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}


using System.Net;

namespace LoanGateway.Auth.Domain.Exceptions;

public sealed class NotFoundException : BaseAppException
{
    public NotFoundException(
        string message = "منبع مورد نظر یافت نشد.",
        int? errorCode= AppErrorCodes.NotFound,
        object? details = null,
        Exception? innerException = null)
        : base(
            message,
            HttpStatusCode.NotFound,
            errorCode ?? 400,
            details,
            innerException)
    {
    }
}


using System.Net;

namespace LoanGateway.Auth.Domain.Exceptions;

public class LogicException : BaseAppException
{
    public LogicException(string message, int? errorCode = AppErrorCodes.LogicError, object? details = null,Exception? innerException = null)
           : base(
               message,
               HttpStatusCode.BadRequest,
               errorCode,
               details,
               innerException)
    {
    }
}

using System.Net;

namespace LoanGateway.Auth.Domain.Exceptions;


public abstract class BaseAppException : Exception
{

    public HttpStatusCode StatusCode { get; }

    public int? ErrorCode { get; }


    public object? Details { get; }

    protected BaseAppException(string message, HttpStatusCode statusCode, int? errorCode = null, object? details = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }
}

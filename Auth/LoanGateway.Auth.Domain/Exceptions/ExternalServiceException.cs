using System.Net;

namespace LoanGateway.Auth.Domain.Exceptions;

public sealed class ExternalServiceException : BaseAppException
{
    public string ExternalSystem { get; }
    public HttpStatusCode ExternalStatusCode { get; }
    public object? Payload { get; }

    public ExternalServiceException(
        string externalSystem,
        HttpStatusCode externalStatusCode,
        string message,
        int? errorCode = AppErrorCodes.ExternalServiceError,
        object? payload = null,
        Exception? innerException = null)
        : base(
            message,
            externalStatusCode,                    
            errorCode ?? 500,
            details: new { externalSystem, externalStatusCode, payload },
            innerException)
    {
        ExternalSystem = externalSystem;
        ExternalStatusCode = externalStatusCode;
        Payload = payload;
    }
}

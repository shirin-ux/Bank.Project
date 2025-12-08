namespace LoanGateway.Auth.Domain.Exceptions;

public class LogicException : Exception
{
    public string? Code { get; }

    public LogicException(string message):base(message)
    {

    }
    public LogicException(string code,string message):base(message)
    {
        Code = code;
    }
}

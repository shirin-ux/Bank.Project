namespace LoanService.Application.Exception;

public class TransientException : System.Exception
{
    public TransientException(string message) : base(message) { }
    public TransientException(string message, System.Exception inner) : base(message, inner) { }

    /// <summary>
    /// مشخص می‌کند که این خطا موقتی است و با retry ممکن است رفع شود.
    /// </summary>
    public static bool IsTransient(System.Exception ex)
    {
        return ex is TimeoutException
            || ex is HttpRequestException
            || ex is TransientException;
    }
}

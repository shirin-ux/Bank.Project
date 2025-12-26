namespace LoanGateway.Auth.Domain.IRepository;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

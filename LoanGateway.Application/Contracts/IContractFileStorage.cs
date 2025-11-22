namespace LoanService.Application.Contracts
{
    public interface IContractFileStorage
    {
        Task<string> SaveAsync(Guid loanId, byte[] fileBytes, string fileName, CancellationToken ct);
        Task<byte[]> ReadAsync(string path, CancellationToken ct = default);
    }
}

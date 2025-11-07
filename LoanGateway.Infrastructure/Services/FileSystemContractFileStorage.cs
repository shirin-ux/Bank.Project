using LoanService.Application.Contracts;
using Microsoft.Extensions.Configuration;

namespace LoanService.Infrastructure.Services;

public sealed class FileSystemContractFileStorage : IContractFileStorage
{
    private readonly string _root;

    public FileSystemContractFileStorage(IConfiguration configuration)
    {
        _root = configuration["Storage:ContractsRoot"]
            ?? throw new InvalidOperationException("Storage:ContractsRoot is not configured.");
    }

    public async Task<string> SaveAsync(Guid loanId, byte[] fileBytes, string fileName, CancellationToken ct)
    {
        var loanFolder = Path.Combine(_root, loanId.ToString("N"));
        Directory.CreateDirectory(loanFolder);

        var fullPath = Path.Combine(loanFolder, fileName);

        await File.WriteAllBytesAsync(fullPath, fileBytes, ct);

        // چیزی که تو دیتابیس می‌ذاری همون fullPath یا relativeه
        return fullPath;
    }
}

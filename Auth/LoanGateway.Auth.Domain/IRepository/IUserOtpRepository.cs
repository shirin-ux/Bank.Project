using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Domain.IRepository
{
    public interface IUserOtpRepository
    {
        Task<int> CountRequestsInWindowAsync(string phoneNumber, OtpPurpose purpose, DateTime utcFrom, CancellationToken ct = default);
        Task<int> CountRequestsByIpInWindowAsync(string requestIp, DateTime utcFrom, CancellationToken ct = default);
        Task<OtpCode> GetActiveAsync(string phoneNumber, OtpPurpose purpose, DateTime nowUtc,CancellationToken ct = default);
        Task InsertAsync(OtpCode otp, CancellationToken ct = default);
        Task UpdateAsync(OtpCode otp, CancellationToken ct = default);
        Task UpdateFailedAttemptsAsync(Guid id, int failedAttempts, CancellationToken ct);
        Task MarkConsumedAsync(Guid id, DateTime consumedAtUtc, CancellationToken ct);
        bool Verify(string code, string phoneNumber, int purpose, byte[] storedHash);
    }
}

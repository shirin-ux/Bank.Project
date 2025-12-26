using LoanService.Domain.Entities.Investment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.IRepository.Investment
{
    public interface IPaymentRepository
    {
        Task CreateAsync(InvestmentPayment row, CancellationToken ct);
        Task<InvestmentPayment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct);

        Task<bool> SetTokenAsync(Guid paymentId, string tokenProtected, byte[] tokenHash, byte[] rowVersion, CancellationToken ct);
        Task<bool> SetCallbackAsync(Guid paymentId, int callbackResCode, byte[] rowVersion, CancellationToken ct);
        Task<bool> SetVerifiedAsync(Guid paymentId, VerifyPayment verify, byte[] rowVersion, CancellationToken ct);
        Task<bool> SetFailedAsync(Guid paymentId, int? verifyResCode, byte[] rowVersion, CancellationToken ct);
    }
}

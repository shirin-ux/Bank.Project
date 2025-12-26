using Microsoft.EntityFrameworkCore;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using LoanService.Infrastructure.Persistence;
using LoanGateway.Auth.Domain;

namespace LoanService.Infrastructure.Repositories.Investment;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly LoanDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentRepository(LoanDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAsync(InvestmentPayment row, CancellationToken ct)
    {
        _context.InvestmentPayments.Add(row);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<InvestmentPayment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct)
    {
        return await _context.InvestmentPayments
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    public async Task<bool> SetTokenAsync(Guid paymentId, string tokenProtected, byte[] tokenHash, byte[] rowVersion, CancellationToken ct)
    {
        var payment = await _context.InvestmentPayments
            .FirstOrDefaultAsync(x => x.Id == paymentId && x.RowVersion.SequenceEqual(rowVersion), ct);

        if (payment == null) return false;

        // Note: You'll need to add these properties to InvestmentPayment entity
        // For now, using reflection or adding properties
        payment.MarkTokenIssued();
       // payment.UpdatedAtUtc = DateTime.UtcNow;

        var result = await _unitOfWork.SaveChangesAsync(ct);
        return result > 0;
    }

    public async Task<bool> SetCallbackAsync(Guid paymentId, int callbackResCode, byte[] rowVersion, CancellationToken ct)
    {
        var payment = await _context.InvestmentPayments
            .FirstOrDefaultAsync(x => x.Id == paymentId && 
                                      x.RowVersion.SequenceEqual(rowVersion) && 
                                      !x.IsFinal, ct);

        if (payment == null) return false;

        payment.MarkCallbackReceived(callbackResCode);
        //payment.UpdatedAtUtc = DateTime.UtcNow;

        var result = await _unitOfWork.SaveChangesAsync(ct);
        return result > 0;
    }

    public async Task<bool> SetVerifiedAsync(Guid paymentId, VerifyPayment verify, byte[] rowVersion, CancellationToken ct)
    {
        var payment = await _context.InvestmentPayments
            .FirstOrDefaultAsync(x => x.Id == paymentId && 
                                      x.RowVersion.SequenceEqual(rowVersion) && 
                                      !x.IsFinal, ct);

        if (payment == null) return false;

        payment.MarkVerified();
        //payment.UpdatedAtUtc = DateTime.UtcNow;

        // Note: You'll need to set VerifyResCode, VerifiedAmountRials, etc. on the entity
        // For now, this is a simplified version

        var result = await _unitOfWork.SaveChangesAsync(ct);
        return result > 0;
    }

    public async Task<bool> SetFailedAsync(Guid paymentId, int? verifyResCode, byte[] rowVersion, CancellationToken ct)
    {
        var payment = await _context.InvestmentPayments
            .FirstOrDefaultAsync(x => x.Id == paymentId && 
                                      x.RowVersion.SequenceEqual(rowVersion) && 
                                      !x.IsFinal, ct);

        if (payment == null) return false;

        payment.MarkFailed();
        //payment.UpdatedAtUtc = DateTime.UtcNow;

        var result = await _unitOfWork.SaveChangesAsync(ct);
        return result > 0;
    }
}

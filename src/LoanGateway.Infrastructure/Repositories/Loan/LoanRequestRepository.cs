using Microsoft.EntityFrameworkCore;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.Enum;
using LoanService.Domain.Enum.Loan;
using LoanService.Domain.IRepository.Loan;
using LoanService.Infrastructure.Persistence;
using LoanService.Domain;
using LoanService.Domain.ValueObjects;

namespace LoanService.Infrastructure.Repositories.Loan;

public class LoanRequestRepository : ILoanRequestRepository
{
    private readonly LoanDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public LoanRequestRepository(LoanDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoanRequest?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct)
    {
        var loan = await _context.LoanRequests
            .Include(x => x.Contract)
            .Include(x => x.Inquiry)
            .Include(x => x.PayResponse)
            .Include(x => x.LastRepayment)
            .Include(x => x.Transfer)
            .Include(x => x.InstallmentStatus)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);

        if (loan != null)
        {
            MapValueObjects(loan);
        }

        return loan;
    }

    public async Task<LoanRequest?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var loan = await _context.LoanRequests
            .Include(x => x.Contract)
            .Include(x => x.Inquiry)
            .Include(x => x.PayResponse)
            .Include(x => x.LastRepayment)
            .Include(x => x.Transfer)
            .Include(x => x.InstallmentStatus)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (loan == null) return null;

        // Map value objects from database columns
        MapValueObjects(loan);

        return loan;
    }

    public async Task InsertAsync(LoanRequest loan, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // Map value objects to database columns before saving
            MapValueObjectsToColumns(loan);

            _context.LoanRequests.Add(loan);

            if (loan.Contract != null)
                _context.Set<ContractInfo>().Add(loan.Contract);

            if (loan.Inquiry != null)
                _context.Set<InquiryInfo>().Add(loan.Inquiry);

            if (loan.PayResponse != null)
                _context.Set<PayResponseInfo>().Add(loan.PayResponse);

            if (loan.LastRepayment != null)
                _context.Set<RepaymentSnapshot>().Add(loan.LastRepayment);

            if (loan.Transfer != null)
                _context.Set<TransferInfo>().Add(loan.Transfer);

            if (loan.InstallmentStatus != null)
                _context.Set<InstallmentStatus>().Add(loan.InstallmentStatus);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task UpdateAsync(LoanRequest loan, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // Map value objects to database columns
            MapValueObjectsToColumns(loan);

            _context.LoanRequests.Update(loan);

            // Handle related entities
            if (loan.Contract != null)
            {
                var existing = await _context.Set<ContractInfo>()
                    .FirstOrDefaultAsync(x => x.LoanRequestId == loan.Id, ct);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(loan.Contract);
                }
                else
                {
                    _context.Set<ContractInfo>().Add(loan.Contract);
                }
            }

            if (loan.Inquiry != null)
            {
                var existing = await _context.Set<InquiryInfo>()
                    .FirstOrDefaultAsync(x => x.LoanRequestId == loan.Id, ct);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(loan.Inquiry);
                }
                else
                {
                    _context.Set<InquiryInfo>().Add(loan.Inquiry);
                }
            }

            if (loan.PayResponse != null)
            {
                var existing = await _context.Set<PayResponseInfo>()
                    .FirstOrDefaultAsync(x => x.LoanRequestId == loan.Id, ct);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(loan.PayResponse);
                }
                else
                {
                    _context.Set<PayResponseInfo>().Add(loan.PayResponse);
                }
            }

            if (loan.LastRepayment != null)
            {
                var existing = await _context.Set<RepaymentSnapshot>()
                    .FirstOrDefaultAsync(x => x.LoanRequestId == loan.Id, ct);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(loan.LastRepayment);
                }
                else
                {
                    _context.Set<RepaymentSnapshot>().Add(loan.LastRepayment);
                }
            }

            if (loan.Transfer != null)
            {
                var existing = await _context.Set<TransferInfo>()
                    .FirstOrDefaultAsync(x => x.LoanRequestId == loan.Id, ct);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(loan.Transfer);
                }
                else
                {
                    _context.Set<TransferInfo>().Add(loan.Transfer);
                }
            }

            if (loan.InstallmentStatus != null)
            {
                var existing = await _context.Set<InstallmentStatus>()
                    .FirstOrDefaultAsync(x => x.LoanRequestId == loan.Id, ct);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(loan.InstallmentStatus);
                }
                else
                {
                    _context.Set<InstallmentStatus>().Add(loan.InstallmentStatus);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task InsertInstallmentAsync(List<InstallmentStatus> installmentStatus, Guid loanId, CancellationToken ct)
    {
        // Delete existing installments
        var existing = await _context.Set<InstallmentStatus>()
            .Where(x => x.LoanRequestId == loanId)
            .ToListAsync(ct);
        
        _context.Set<InstallmentStatus>().RemoveRange(existing);

        // Add new installments
        foreach (var item in installmentStatus)
        {
            item.Id = Guid.NewGuid();
            item.LoanRequestId = loanId;
            _context.Set<InstallmentStatus>().Add(item);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private void MapValueObjects(LoanRequest loan)
    {
        var entry = _context.Entry(loan);

        // Map Provider
        var providerType = entry.Property("Provider_Type").CurrentValue as int?;
        var providerApprovalCode = entry.Property("Provider_ApprovalCode").CurrentValue as decimal?;
        var providerRequiresOtp = entry.Property("Provider_RequiresOtp").CurrentValue as bool?;

        if (providerType.HasValue)
        {
            loan.Provider = new ProviderInfo(
                (ProviderType)providerType.Value,
                providerApprovalCode,
                providerRequiresOtp ?? false
            );
        }

        // Map InqueryRequest
        var inquiryRequestId = entry.Property("InquiryRequest_Id").CurrentValue as string;
        loan.InqueryRequest = new InqueryRequest(inquiryRequestId);

        // Map PayRequest
        var payRequestId = entry.Property("PayRequest_Id").CurrentValue as string;
        var payRequestAmount = entry.Property("PayRequest_RequestedAmount").CurrentValue as decimal?;
        loan.PayRequest = new PayRequestInfo(payRequestId, payRequestAmount);

        // Map LastDecision - Note: DecisionStamp parameter order is ReasonCode, ReasonMessage, ErrorCode, ErrorMessage
        var decisionReasonCode = entry.Property("Decision_ReasonCode").CurrentValue as int?;
        var decisionReasonMessage = entry.Property("Decision_ReasonMessage").CurrentValue as string;
        var decisionErrorCode = entry.Property("Decision_ErrorCode").CurrentValue as int?;
        var decisionErrorMessage = entry.Property("Decision_ErrorMessage").CurrentValue as string;
        loan.LastDecision = new DecisionStamp(
            decisionReasonCode,
            decisionReasonMessage,
            decisionErrorCode,
            decisionErrorMessage
        );

        // Map GrantRequest
        var grantContractId = entry.Property("Grant_ContractId").CurrentValue as decimal?;
        var grantStatus = entry.Property("Grant_Status").CurrentValue as string;
        var grantPayRequestId = entry.Property("PayRequest_Id").CurrentValue as string;
        var grantRequestedAmount = entry.Property("Grant_RequestedAmount").CurrentValue as decimal?;
        var grantSignedContract = entry.Property("Grant_SignedContractBase64").CurrentValue as string;
        loan.GrantRequest = new GrantRequest(
            grantContractId,
            grantStatus,
            grantPayRequestId,
            grantRequestedAmount?.ToString(),
            grantSignedContract
        );
    }

    private void MapValueObjectsToColumns(LoanRequest loan)
    {
        var entry = _context.Entry(loan);

        // Map Provider to columns
        if (loan.Provider != null)
        {
            entry.Property("Provider_Type").CurrentValue = (int)loan.Provider.ProviderType;
            entry.Property("Provider_ApprovalCode").CurrentValue = loan.Provider.ApprovalCode;
            entry.Property("Provider_RequiresOtp").CurrentValue = loan.Provider.RequiresOtp;
        }

        // Map InqueryRequest to columns
        entry.Property("InquiryRequest_Id").CurrentValue = loan.InqueryRequest?.RequestId;

        // Map PayRequest to columns
        if (loan.PayRequest != null)
        {
            entry.Property("PayRequest_Id").CurrentValue = loan.PayRequest.PayRequestId;
            entry.Property("PayRequest_RequestedAmount").CurrentValue = loan.PayRequest.RequestedAmount;
        }

        // Map LastDecision to columns
        if (loan.LastDecision != null)
        {
            entry.Property("Decision_ReasonCode").CurrentValue = loan.LastDecision.ReasonCode;
            entry.Property("Decision_ReasonMessage").CurrentValue = loan.LastDecision.ReasonMessage;
            entry.Property("Decision_ErrorCode").CurrentValue = loan.LastDecision.ErrorCode;
            entry.Property("Decision_ErrorMessage").CurrentValue = loan.LastDecision.ErrorMessage;
        }

        // Map GrantRequest to columns
        if (loan.GrantRequest != null)
        {
            entry.Property("Grant_ContractId").CurrentValue = loan.GrantRequest.ContractId;
            entry.Property("Grant_Status").CurrentValue = loan.GrantRequest.Status;
            entry.Property("Grant_RequestedAmount").CurrentValue = decimal.TryParse(loan.GrantRequest.RequestedAmount, out var amount) ? amount : (decimal?)null;
            entry.Property("Grant_SignedContractBase64").CurrentValue = loan.GrantRequest.SignedContractBase64;
        }
    }
}

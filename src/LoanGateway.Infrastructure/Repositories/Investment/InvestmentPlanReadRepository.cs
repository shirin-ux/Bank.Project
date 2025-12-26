using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using LoanService.Infrastructure.Persistence;
using LoanService.Domain;

namespace LoanService.Infrastructure.Repositories.Investment;

public class InvestmentPlanReadRepository : IInvestmentPlanReadRepository
{
    private readonly LoanDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public InvestmentPlanReadRepository(LoanDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> DeletAsync(InvestmentPlans plan, CancellationToken ct)
    {
        var existing = await _context.InvestmentPlans.FindAsync(new object[] { plan.Id }, ct);
        if (existing != null)
        {
            // Use shadow properties for IsDeleted and IsActive
            var entry = _context.Entry(existing);
            entry.Property("IsDeleted").CurrentValue = true;
            entry.Property("IsActive").CurrentValue = false;
            entry.Property("UpdatedAtUtc").CurrentValue = DateTime.UtcNow;
            
            return await _unitOfWork.SaveChangesAsync(ct) > 0;
        }
        return false;
    }

    public async Task<IEnumerable<InvestmentPlans>> GetActivePlansAsync(CancellationToken ct)
    {
        return await _context.InvestmentPlans
            .Where(x => EF.Property<bool>(x, "IsActive") == true && 
                       EF.Property<bool>(x, "IsDeleted") == false)
            .Include(x => x.Features)
            .Include(x => x.Faqs)
            .OrderByDescending(x => x.Id)
            .ToListAsync(ct);
    }

    public async Task<InvestmentPlans> GetByCodeAsync(string Code, CancellationToken ct)
    {
        return await _context.InvestmentPlans
            .Where(x => EF.Property<string>(x, "Code") == Code && 
                       EF.Property<bool>(x, "IsDeleted") == false)
            .Include(x => x.Features)
            .Include(x => x.Faqs)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<InvestmentPlans> GetByIdAsync(Guid Id, CancellationToken ct)
    {
        return await _context.InvestmentPlans
            .Where(x => x.Id == Id && EF.Property<bool>(x, "IsDeleted") == false)
            .Include(x => x.Features)
            .Include(x => x.Faqs)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<InvestmentPlans?> GetPlanWithMetaAsync(InvestmentPlanType planType, CancellationToken ct)
    {
        var plan = await _context.InvestmentPlans
            .Include(x => x.Features.OrderBy(f => f.Order))
            .Include(x => x.Faqs.OrderBy(f => f.Order))
            .FirstOrDefaultAsync(x => x.PlanType == planType, ct);

        return plan;
    }

    public async Task<Guid> InsertAsync(InvestmentPlans plan, CancellationToken ct)
    {
        _context.InvestmentPlans.Add(plan);
        await _unitOfWork.SaveChangesAsync(ct);
        return plan.Id;
    }

    public async Task<bool> UpdateAsync(InvestmentPlans plan, CancellationToken ct)
    {
        var existing = await _context.InvestmentPlans
            .Where(x => x.Id == plan.Id && EF.Property<bool>(x, "IsDeleted") == false)
            .FirstOrDefaultAsync(ct);
            
        if (existing != null)
        {
            var entry = _context.Entry(existing);
            entry.CurrentValues.SetValues(plan);
            entry.Property("UpdatedAtUtc").CurrentValue = DateTime.UtcNow;
            return await _unitOfWork.SaveChangesAsync(ct) > 0;
        }
        return false;
    }

    public async Task UpsertDailyHistoryAsync(InvestmentPlanType plan, IReadOnlyList<InvestmentIndexHistory> points, CancellationToken ct)
    {
        if (points == null || points.Count == 0)
            return;

        var daily = points
            .GroupBy(p => p.IndexDateTimeUtc.Date)
            .Select(g => new InvestmentIndexHistory
            {
                PlanType = plan,
                IndexDateTimeUtc = g.Key,
                IndexValue = g.OrderBy(x => x.IndexDateTimeUtc).Last().IndexValue
            })
            .ToList();

        if (!daily.Any())
            return;

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            foreach (var item in daily)
            {
                var existing = await _context.InvestmentIndexHistories
                    .FirstOrDefaultAsync(x => 
                        x.PlanType == plan && 
                        x.IndexDateTimeUtc.Date == item.IndexDateTimeUtc.Date, 
                        ct);

                if (existing != null)
                {
                    existing.IndexValue = item.IndexValue;
                    _context.InvestmentIndexHistories.Update(existing);
                }
                else
                {
                    _context.InvestmentIndexHistories.Add(item);
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

    public async Task<IReadOnlyList<InvestmentIndexHistory>> GetRangeAsync(InvestmentPlanType plan, DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct)
    {
        return await _context.InvestmentIndexHistories
            .Where(x => x.PlanType == plan && 
                       x.IndexDateTimeUtc >= fromDateUtc && 
                       x.IndexDateTimeUtc <= toDateUtc)
            .OrderBy(x => x.IndexDateTimeUtc)
            .ToListAsync(ct);
    }

    public async Task<InvestmentAccount?> GetByNationalCodeAndPlanAsync(string nationalCode, InvestmentPlanType planType, CancellationToken ct)
    {
        // Note: Operations is a private collection, so we can't Include it directly
        // The entity will need to be loaded and operations populated separately if needed
        return await _context.InvestmentAccounts
            .Where(x => x.NationalCode == nationalCode && x.PlanCode == planType)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(InvestmentAccount account, CancellationToken ct)
    {
        _context.InvestmentAccounts.Add(account);
        
        // Add operations if any
        if (account.Operations.Any())
        {
            foreach (var operation in account.Operations)
            {
                _context.InvestmentOperations.Add(operation);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<bool> CheckReceiptNumberExistsAsync(Guid? policyId, string receiptNumber, CancellationToken ct)
    {
        if (policyId == null || string.IsNullOrWhiteSpace(receiptNumber))
            return false;

        return await _context.InvestmentOperations
            .AnyAsync(x => x.PolicyId == policyId && x.ReceiptNumber == receiptNumber, ct);
    }

    public async Task<bool> UpdateAsync(InvestmentAccount account, CancellationToken ct)
    {
        var existing = await _context.InvestmentAccounts
            .FirstOrDefaultAsync(x => x.Id == account.Id, ct);

        if (existing != null)
        {
            // Update account properties
            var entry = _context.Entry(existing);
            entry.CurrentValues.SetValues(account);
            entry.Property("UpdatedAtUtc").CurrentValue = DateTime.UtcNow;
            
            // Note: Operations is a private collection, so we can't update it directly
            // You might need to handle operations separately or modify the entity design
            
            return await _unitOfWork.SaveChangesAsync(ct) > 0;
        }
        return false;
    }

    public async Task<DateTime?> GetLastDateAsync(InvestmentPlanType plan, CancellationToken ct)
    {
        return await _context.InvestmentIndexHistories
            .Where(x => x.PlanType == plan)
            .OrderByDescending(x => x.IndexDateTimeUtc)
            .Select(x => x.IndexDateTimeUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task DeleteOlderThanAsync(InvestmentPlanType plan, DateTime cutoffUtc, CancellationToken ct)
    {
        var toDelete = await _context.InvestmentIndexHistories
            .Where(x => x.PlanType == plan && x.IndexDateTimeUtc < cutoffUtc.Date)
            .ToListAsync(ct);

        _context.InvestmentIndexHistories.RemoveRange(toDelete);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<InvestmentPlans> GetPlanAsync(string planStatus, CancellationToken ct)
    {
        
        return await _context.InvestmentPlans
            .Where(x => EF.Property<bool>(x, "IsActive") == true && 
                       EF.Property<bool>(x, "IsDeleted") == false)
            .Include(x => x.Features)
            .Include(x => x.Faqs)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<InvestmentIndexHistory>> GetLatestPointsAsync(InvestmentPlanType planType, int count, CancellationToken ct)
    {
        return await _context.InvestmentIndexHistories
            .Where(x => x.PlanType == planType)
            .OrderByDescending(x => x.IndexDateTimeUtc)
            .Take(count)
            .ToListAsync(ct);
    }
}

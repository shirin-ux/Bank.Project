using Microsoft.EntityFrameworkCore;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.Entities.Investment;

namespace LoanService.Infrastructure.Persistence;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
    }

    // Loan entities
    public DbSet<LoanRequest> LoanRequests { get; set; }
    public DbSet<ContractInfo> ContractInfos { get; set; }
    public DbSet<InquiryInfo> InquiryInfos { get; set; }
    public DbSet<PayResponseInfo> PayResponseInfos { get; set; }
    public DbSet<RepaymentSnapshot> RepaymentSnapshots { get; set; }
    public DbSet<TransferInfo> TransferInfos { get; set; }
    public DbSet<InstallmentStatus> InstallmentStatuses { get; set; }
    public DbSet<InvestmentIndexHistory> InvestmentIndexHistories { get; set; }

    // Investment entities
    public DbSet<InvestmentAccount> InvestmentAccounts { get; set; }
    public DbSet<InvestmentOperation> InvestmentOperations { get; set; }
    public DbSet<InvestmentPayment> InvestmentPayments { get; set; }
    public DbSet<InvestmentPlans> InvestmentPlans { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

   
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoanDbContext).Assembly);
    }
}


using Microsoft.EntityFrameworkCore;
using LoanGateway.Auth.Domain.Entities;
using LoanService.Domain.Entities.Investment;

namespace LoanService.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }
    public DbSet<LoanGateway.Auth.Domain.Entities.GiftCardEligibleUser> GiftCardEligibleUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}


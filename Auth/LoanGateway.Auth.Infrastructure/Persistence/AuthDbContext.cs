using Microsoft.EntityFrameworkCore;
using LoanGateway.Auth.Domain.Entities;

namespace LoanGateway.Auth.Infrastructure.Persistence;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<RefreshTokens> RefreshTokens { get; set; }
    public DbSet<VerfiyMobileOwnerInquiry> VerfiyMobileOwnerInquiries { get; set; }
    public DbSet<GiftCardEligibleUser> GiftCardEligibleUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}


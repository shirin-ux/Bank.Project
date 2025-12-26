using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class InvestmentAccountConfiguration : IEntityTypeConfiguration<InvestmentAccount>
{
    public void Configure(EntityTypeBuilder<InvestmentAccount> builder)
    {
        builder.ToTable("InvestmentAccount");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderPolicyId);

        builder.Property(x => x.NationalCode)
            .HasMaxLength(10);

        builder.Property(x => x.BirthDate)
            .HasMaxLength(50);

        builder.Property(x => x.PlanCode)
            .HasConversion<byte?>();

        builder.Property(x => x.PostalCode)
            .HasMaxLength(10);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.State)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.TotalInvested)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.TotalWithdrawn)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CurrentValue)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.RevokableAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.CollateralAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.LastTraceId)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Relationships
        // Note: Operations might be stored separately, so we'll configure it if needed
        // builder.HasMany(x => x.Operations)
        //     .WithOne()
        //     .HasForeignKey("AccountId")
        //     .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.NationalCode);
        builder.HasIndex(x => new { x.NationalCode, x.PlanCode });
    }
}


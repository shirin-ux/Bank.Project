using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class InvestmentIndexHistoryConfiguration : IEntityTypeConfiguration<InvestmentIndexHistory>
{
    public void Configure(EntityTypeBuilder<InvestmentIndexHistory> builder)
    {
        builder.ToTable("InvestmentIndexHistory");

        builder.HasKey(x => new { x.PlanType, x.IndexDateTimeUtc });

        builder.Property(x => x.PlanType)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.IndexDateTimeUtc)
            .IsRequired();

        builder.Property(x => x.IndexValue)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.HasIndex(x => new { x.PlanType, x.IndexDateTimeUtc });
    }
}


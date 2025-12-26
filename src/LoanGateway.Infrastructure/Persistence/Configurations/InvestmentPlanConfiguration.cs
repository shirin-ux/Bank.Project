using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class InvestmentPlanConfiguration : IEntityTypeConfiguration<InvestmentPlans>
{
    public void Configure(EntityTypeBuilder<InvestmentPlans> builder)
    {
        builder.ToTable("InvestmentPlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanType)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.MinAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.FixedIncomeAnnualRatePercent)
            .HasColumnType("decimal(18,2)");

        // Shadow properties for database columns that might not exist in entity
        builder.Property<bool>("IsActive")
            .HasDefaultValue(true);

        builder.Property<bool>("IsDeleted")
            .HasDefaultValue(false);

        builder.Property<string>("Code")
            .HasMaxLength(50);

        builder.Property<string>("Name")
            .HasMaxLength(200);

        builder.Property<string>("ShortDescription")
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasMany(x => x.Features)
            .WithOne()
            .HasForeignKey("PlanId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Faqs)
            .WithOne()
            .HasForeignKey("PlanId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PlanType);
    }
}


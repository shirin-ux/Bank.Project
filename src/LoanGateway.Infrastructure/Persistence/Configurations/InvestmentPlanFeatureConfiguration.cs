using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Investment;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class InvestmentPlanFeatureConfiguration : IEntityTypeConfiguration<InvestmentPlanFeature>
{
    public void Configure(EntityTypeBuilder<InvestmentPlanFeature> builder)
    {
        builder.ToTable("InvestmentPlanFeatures");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.PlanId);
        builder.HasIndex(x => new { x.PlanId, x.Order });
    }
}



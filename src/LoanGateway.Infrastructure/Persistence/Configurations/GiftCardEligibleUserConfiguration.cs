using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanGateway.Auth.Domain.Entities;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class GiftCardEligibleUserConfiguration : IEntityTypeConfiguration<GiftCardEligibleUser>
{
    public void Configure(EntityTypeBuilder<GiftCardEligibleUser> builder)
    {
        builder.ToTable("GiftCardEligibleUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NationalCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ProcessedAtUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.NationalCode);
    }
}


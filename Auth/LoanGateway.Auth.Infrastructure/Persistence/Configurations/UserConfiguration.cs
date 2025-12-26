using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.Enum;

namespace LoanGateway.Auth.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MobileNumber)
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(x => x.NationalCode)
            .HasMaxLength(10);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(10);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.Property(x => x.BirthDate)
            .HasMaxLength(50);

        builder.Property(x => x.ShenasnamehNumber)
            .HasMaxLength(50);

        builder.Property(x => x.ShenasnameSerial)
            .HasMaxLength(50);

        builder.Property(x => x.ShenasnameSeri)
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.UidUserId)
            .HasMaxLength(100);

        builder.Property(x => x.IsMobileVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsKycCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsProfileCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsKycVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.KycLevel)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.GiftStatus)
            .HasDefaultValue(false);

        // IsDeleted might not exist in all databases, so we'll handle it gracefully
        builder.Ignore(x => x.IsDeleted);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.MobileNumber);
        builder.HasIndex(x => x.NationalCode);
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanGateway.Auth.Domain.Entities;

namespace LoanGateway.Auth.Infrastructure.Persistence.Configurations;

public class VerfiyMobileOwnerInquiryConfiguration : IEntityTypeConfiguration<VerfiyMobileOwnerInquiry>
{
    public void Configure(EntityTypeBuilder<VerfiyMobileOwnerInquiry> builder)
    {
        builder.ToTable("VerfiyMobileOwnerInquiry");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NationalId)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.MobileNumber)
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(x => x.IsMatched);

        builder.Property(x => x.StatusCode);

        builder.Property(x => x.StatusMessage)
            .HasMaxLength(500);

        builder.Property(x => x.RequestId)
            .HasMaxLength(100);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder.Property(x => x.RawResponseJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.NationalId);
        builder.HasIndex(x => x.MobileNumber);
    }
}



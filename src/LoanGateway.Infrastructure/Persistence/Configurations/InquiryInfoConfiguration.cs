using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.Enum.Loan;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class InquiryInfoConfiguration : IEntityTypeConfiguration<InquiryInfo>
{
    public void Configure(EntityTypeBuilder<InquiryInfo> builder)
    {
        builder.ToTable("InquiryInfo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LoanRequestId)
            .IsRequired();

        builder.Property(x => x.Allowed);

        builder.Property(x => x.MaxApprovedAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Ics);

        builder.Property(x => x.IcsGrade)
            .HasConversion<int?>();

        builder.Property(x => x.ExpireAt)
            .HasMaxLength(50);

        builder.Ignore(x => x.Statuses);

        builder.HasIndex(x => x.LoanRequestId)
            .IsUnique();
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Loan;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class RepaymentSnapshotConfiguration : IEntityTypeConfiguration<RepaymentSnapshot>
{
    public void Configure(EntityTypeBuilder<RepaymentSnapshot> builder)
    {
        builder.ToTable("RepaymentSnapshot");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LoanRequestId)
            .IsRequired();

        builder.Property(x => x.TrackNumber)
            .HasMaxLength(100);

        builder.Property(x => x.AccountNo)
            .HasMaxLength(50);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.IcsGrade)
            .HasConversion<int?>();

        builder.Property(x => x.WhenUtc);

        builder.HasIndex(x => x.LoanRequestId)
            .IsUnique();
    }
}


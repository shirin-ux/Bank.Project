using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Loan;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class TransferInfoConfiguration : IEntityTypeConfiguration<TransferInfo>
{
    public void Configure(EntityTypeBuilder<TransferInfo> builder)
    {
        builder.ToTable("TransferInfo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LoanRequestId)
            .IsRequired();

        builder.Property(x => x.RegisterCode)
            .HasMaxLength(100);

        builder.Property(x => x.TransactionNumber)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.LoanRequestId)
            .IsUnique();
    }
}



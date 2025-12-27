using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Loan;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class InstallmentStatusConfiguration : IEntityTypeConfiguration<InstallmentStatus>
{
    public void Configure(EntityTypeBuilder<InstallmentStatus> builder)
    {
        builder.ToTable("InstallmentStatus");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LoanRequestId)
            .IsRequired();

        builder.Property(x => x.ContractNumber)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.InstallmentNo)
            .IsRequired();

        builder.Property(x => x.NationalCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.DueDate)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.PaidAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50);

        builder.HasIndex(x => x.LoanRequestId);
    }
}



using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.Enum.Loan;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class ContractInfoConfiguration : IEntityTypeConfiguration<ContractInfo>
{
    public void Configure(EntityTypeBuilder<ContractInfo> builder)
    {
        builder.ToTable("ContractInfo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LoanRequestId)
            .IsRequired();

        builder.Property(x => x.ApprovalCode)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ContractNumber)
            .HasMaxLength(100);

        builder.Property(x => x.NationalCode)
            .HasMaxLength(10);

        builder.Property(x => x.BirthDate);

        builder.Property(x => x.MobileNumber)
            .HasMaxLength(11);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(10);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(11);

        builder.Property(x => x.LoanAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.InstallmentCount);

        builder.Property(x => x.CollateralType)
            .HasConversion<int>();

        builder.Property(x => x.CollateralNo)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.CollateralDate)
            .HasMaxLength(50);

        builder.Property(x => x.CollateralAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.GuarantorNC)
            .HasMaxLength(10);

        builder.Property(x => x.CollateralIssuer)
            .HasMaxLength(200);

        builder.Property(x => x.ContractPath)
            .HasMaxLength(500);

        builder.Property(x => x.ChequeSerial)
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.cbTrackingCode)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.LoanRequestId)
            .IsUnique();
    }
}


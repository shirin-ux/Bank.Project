using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Loan;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class PayResponseInfoConfiguration : IEntityTypeConfiguration<PayResponseInfo>
{
    public void Configure(EntityTypeBuilder<PayResponseInfo> builder)
    {
        builder.ToTable("PayResponseInfo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.LoanRequestId)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.BankContractNo)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ApprovedLoanAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ContractDate)
            .HasMaxLength(50);

        builder.Property(x => x.CentralBankTraceCode)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.BankSignedContractBase64)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ReceivedAtUtc);

        builder.HasIndex(x => x.LoanRequestId)
            .IsUnique();
    }
}



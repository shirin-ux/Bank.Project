using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class InvestmentOperationConfiguration : IEntityTypeConfiguration<InvestmentOperation>
{
    public void Configure(EntityTypeBuilder<InvestmentOperation> builder)
    {
        builder.ToTable("InvestmentOperation");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PolicyId);

        builder.Property(x => x.TraceId)
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.OperationDate)
            .IsRequired();

        builder.Property(x => x.ReceiptDate);

        builder.Property(x => x.ReceiptNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.PolicyId);
        builder.HasIndex(x => x.TraceId);
        builder.HasIndex(x => new { x.PolicyId, x.ReceiptNumber });
    }
}



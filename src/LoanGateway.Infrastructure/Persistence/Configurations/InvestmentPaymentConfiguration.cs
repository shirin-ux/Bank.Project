using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class InvestmentPaymentConfiguration : IEntityTypeConfiguration<InvestmentPayment>
{
    public void Configure(EntityTypeBuilder<InvestmentPayment> builder)
    {
        builder.ToTable("InvestmentPayment");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.AmountRials)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.IsFinal)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CallbackResCode);

        builder.Property(x => x.VerifyResCode);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsRequired();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();
    }
}



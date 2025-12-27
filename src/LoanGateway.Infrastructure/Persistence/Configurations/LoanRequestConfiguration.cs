using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.Enum.Loan;

namespace LoanService.Infrastructure.Persistence.Configurations;

public class LoanRequestConfiguration : IEntityTypeConfiguration<LoanRequest>
{
    public void Configure(EntityTypeBuilder<LoanRequest> builder)
    {
        builder.ToTable("LoanRequest");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.State)
            .HasConversion<int>()
            .IsRequired()
            .HasColumnName("State");

        builder.Property(x => x.RequiresOtp)
            .IsRequired()
            .HasColumnName("RequiresOtp");

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasColumnName("UserId");

        builder.Property(x => x.RequestAmount)
            .HasConversion<int>()
            .HasColumnName("RequestAmount");

        builder.Property(x => x.InstallmentCount)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue((int)LoanService.Domain.Enum.Loan.InstallmentCount.TwelveMonths)
            .HasColumnName("InstallmentCount");

        builder.Property(x => x.RequiresCollateral)
            .IsRequired()
            .HasColumnName("RequiresCollateral");

        builder.Property(x => x.CollateralType)
            .HasConversion<int>()
            .HasColumnName("CollateralType");

        builder.Property(x => x.CorrelationId)
            .IsRequired()
            .HasColumnName("CorrelationId");

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(100)
            .HasColumnName("IdempotencyKey");

        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("RetryCount");

        // Value Objects stored as separate columns (matching existing DB structure)
        // Use shadow properties for database columns that map to value objects
        builder.Ignore(x => x.Provider);
        builder.Ignore(x => x.InqueryRequest);
        builder.Ignore(x => x.PayRequest);
        builder.Ignore(x => x.LastDecision);
        builder.Ignore(x => x.GrantRequest);

        builder.Property<int?>("Provider_Type")
            .HasColumnName("Provider_Type");

        builder.Property<decimal?>("Provider_ApprovalCode")
            .HasColumnType("decimal(18,2)")
            .HasColumnName("Provider_ApprovalCode");

        builder.Property<bool?>("Provider_RequiresOtp")
            .HasColumnName("Provider_RequiresOtp");

        builder.Property<string>("InquiryRequest_Id")
            .HasColumnName("InquiryRequest_Id");

        builder.Property<string>("PayRequest_Id")
            .HasColumnName("PayRequest_Id");

        builder.Property<decimal?>("PayRequest_RequestedAmount")
            .HasColumnType("decimal(18,2)")
            .HasColumnName("PayRequest_RequestedAmount");

        builder.Property<int?>("Decision_ErrorCode")
            .HasColumnName("Decision_ErrorCode");

        builder.Property<string>("Decision_ErrorMessage")
            .HasColumnName("Decision_ErrorMessage");

        builder.Property<int?>("Decision_ReasonCode")
            .HasColumnName("Decision_ReasonCode");

        builder.Property<string>("Decision_ReasonMessage")
            .HasColumnName("Decision_ReasonMessage");

        builder.Property<decimal?>("Grant_ContractId")
            .HasColumnType("decimal(18,2)")
            .HasColumnName("Grant_ContractId");

        builder.Property<string>("Grant_Status")
            .HasColumnName("Grant_Status");

        builder.Property<decimal?>("Grant_RequestedAmount")
            .HasColumnType("decimal(18,2)")
            .HasColumnName("Grant_RequestedAmount");

        builder.Property<string>("Grant_SignedContractBase64")
            .HasColumnName("Grant_SignedContractBase64");

        builder.Property(x => x.LastReasonCode)
            .HasColumnName("LastReasonCode");

        builder.Property(x => x.LastReasonMessage)
            .HasColumnName("LastReasonMessage");

        builder.Property(x => x.LastErrorCode)
            .HasColumnName("LastErrorCode");

        builder.Property(x => x.LastErrorMessage)
            .HasColumnName("LastErrorMessage");

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasColumnName("CreatedAtUtc");

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired()
            .HasColumnName("UpdatedAtUtc");

        // Relationships
        builder.HasOne(x => x.Contract)
            .WithOne()
            .HasForeignKey<ContractInfo>(x => x.LoanRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Inquiry)
            .WithOne()
            .HasForeignKey<InquiryInfo>(x => x.LoanRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PayResponse)
            .WithOne()
            .HasForeignKey<PayResponseInfo>(x => x.LoanRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LastRepayment)
            .WithOne()
            .HasForeignKey<RepaymentSnapshot>(x => x.LoanRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Transfer)
            .WithOne()
            .HasForeignKey<TransferInfo>(x => x.LoanRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InstallmentStatus)
            .WithOne()
            .HasForeignKey<InstallmentStatus>(x => x.LoanRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

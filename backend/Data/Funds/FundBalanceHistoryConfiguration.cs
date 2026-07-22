using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Funds;

/// <summary>
/// EF Core entity configuration for a <see cref="FundBalanceHistory"/>
/// </summary>
internal sealed class FundBalanceHistoryConfiguration : IEntityTypeConfiguration<FundBalanceHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundBalanceHistory> builder)
    {
        builder.HasKey(fundBalanceHistory => fundBalanceHistory.Id);
        builder.Property(fundBalanceHistory => fundBalanceHistory.Id).HasConversion(fundBalanceHistoryId => fundBalanceHistoryId.Value, value => new FundBalanceHistoryId(value));

        builder.Property<FundId>("FundId")
            .IsRequired()
            .HasConversion(id => id.Value, value => new FundId(value));
        builder.HasOne(fundBalanceHistory => fundBalanceHistory.Fund).WithMany().HasForeignKey("FundId");
        builder.Navigation(fundBalanceHistory => fundBalanceHistory.Fund).AutoInclude();
        builder.HasIndex("FundId", nameof(FundBalanceHistory.Date), nameof(FundBalanceHistory.Sequence)).IsUnique();

        builder.Property(fundBalanceHistory => fundBalanceHistory.TransactionId)
            .HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));
    }
}
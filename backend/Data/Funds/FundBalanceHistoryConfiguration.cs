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

        builder.Property(fundBalanceHistory => fundBalanceHistory.FundId)
            .HasConversion(fundId => fundId.Value, value => new FundId(value));

        builder.Property(fundBalanceHistory => fundBalanceHistory.TransactionId)
            .HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));
    }
}
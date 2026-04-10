using Domain.AccountingPeriods;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Transactions;

/// <summary>
/// EF Core entity configuration for a <see cref="Transaction"/>
/// </summary>
internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(transaction => transaction.Id);
        builder.Property(transaction => transaction.Id).HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));

        builder.Property(transaction => transaction.AccountingPeriodId)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasDiscriminator(transaction => transaction.Type)
            .HasValue<SpendingTransaction>(TransactionType.Spending)
            .HasValue<SpendingTransferTransaction>(TransactionType.SpendingTransfer)
            .HasValue<IncomeTransaction>(TransactionType.Income)
            .HasValue<IncomeTransferTransaction>(TransactionType.IncomeTransfer)
            .HasValue<TransferTransaction>(TransactionType.Transfer);
        // .HasValue<RefundTransaction>(TransactionType.Refund);
    }
}

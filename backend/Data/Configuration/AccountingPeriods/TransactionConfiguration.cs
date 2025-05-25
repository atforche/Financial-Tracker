using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.AccountingPeriods;

/// <summary>
/// EF Core entity configuration for a <see cref="Transaction"/>
/// </summary>
internal sealed class TransactionConfiguration : EntityConfiguration<Transaction>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasMany(transaction => transaction.AccountingEntries)
            .WithOne()
            .HasForeignKey("TransactionId");
        builder.Navigation(transaction => transaction.AccountingEntries).AutoInclude();

        builder.HasMany(transaction => transaction.TransactionBalanceEvents)
            .WithOne(transactionBalanceEvent => transactionBalanceEvent.Transaction)
            .HasForeignKey("TransactionId");
        builder.Navigation(transaction => transaction.TransactionBalanceEvents).AutoInclude();
    }
}
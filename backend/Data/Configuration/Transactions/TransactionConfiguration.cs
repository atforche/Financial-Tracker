using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Transactions;

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

        builder.HasOne<AccountingPeriod>().WithMany().HasForeignKey(transaction => transaction.AccountingPeriodId);

        builder.HasOne<Account>().WithMany().HasForeignKey(transaction => transaction.DebitAccountId);
        builder.HasOne<Account>().WithMany().HasForeignKey(transaction => transaction.CreditAccountId);

        builder.HasMany(transaction => transaction.DebitFundAmounts)
            .WithOne()
            .HasForeignKey("DebitTransactionId");
        builder.Navigation(transaction => transaction.DebitFundAmounts).AutoInclude();

        builder.HasMany(transaction => transaction.CreditFundAmounts)
            .WithOne()
            .HasForeignKey("CreditTransactionId");
        builder.Navigation(transaction => transaction.CreditFundAmounts).AutoInclude();

        builder.HasMany(transaction => transaction.TransactionBalanceEvents)
            .WithOne(transactionBalanceEvent => transactionBalanceEvent.Transaction)
            .HasForeignKey("TransactionId");
        builder.Navigation(transaction => transaction.TransactionBalanceEvents).AutoInclude();
    }
}
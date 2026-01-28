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

        builder.Property(transaction => transaction.AccountingPeriod)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasOne(transaction => transaction.DebitAccount).WithMany().HasForeignKey("DebitAccountId");
        builder.Navigation(transaction => transaction.DebitAccount).AutoInclude();

        builder.HasOne(transaction => transaction.CreditAccount).WithMany().HasForeignKey("CreditAccountId");
        builder.Navigation(transaction => transaction.CreditAccount).AutoInclude();

        builder.Property(transaction => transaction.InitialAccountTransaction)
            .HasConversion(accountingPeriodId => accountingPeriodId == null ? (Guid?)null : accountingPeriodId.Value,
                value => value == null ? null : new AccountId(value.Value));
    }
}
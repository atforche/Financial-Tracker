using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
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

        builder.Property(transaction => transaction.AccountingPeriod)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.OwnsOne(transaction => transaction.DebitAccount, builder =>
        {
            builder.WithOwner(transactionAccount => transactionAccount.Transaction);
            builder.Navigation(transactionAccount => transactionAccount.Transaction).AutoInclude();

            builder.HasOne<Account>().WithMany().HasForeignKey(transactionAccount => transactionAccount.AccountId);

            builder.OwnsMany(transactionAccount => transactionAccount.FundAmounts, fundAmount =>
            {
                fundAmount.ToTable("TransactionDebitAccountFundAmounts");
                fundAmount.Property<int>("Id");
                fundAmount.HasKey("Id");

                fundAmount.Property(fundAmount => fundAmount.FundId)
                    .HasConversion(fundId => fundId.Value, value => new FundId(value));
            });
            builder.Navigation(transactionAccount => transactionAccount.FundAmounts).AutoInclude();
        });
        builder.Navigation(transaction => transaction.DebitAccount).AutoInclude();

        builder.OwnsOne(transaction => transaction.CreditAccount, builder =>
        {
            builder.WithOwner(transactionAccount => transactionAccount.Transaction);
            builder.Navigation(transactionAccount => transactionAccount.Transaction).AutoInclude();

            builder.HasOne<Account>().WithMany().HasForeignKey(transactionAccount => transactionAccount.AccountId);

            builder.OwnsMany(transactionAccount => transactionAccount.FundAmounts, fundAmount =>
            {
                fundAmount.ToTable("TransactionCreditAccountFundAmounts");
                fundAmount.Property<int>("Id");
                fundAmount.HasKey("Id");

                fundAmount.Property(fundAmount => fundAmount.FundId)
                    .HasConversion(fundId => fundId.Value, value => new FundId(value));
            });
            builder.Navigation(transactionAccount => transactionAccount.FundAmounts).AutoInclude();
        });
        builder.Navigation(transaction => transaction.CreditAccount).AutoInclude();

        builder.Property(transaction => transaction.GeneratedByAccountId)
            .HasConversion(accountingPeriodId => accountingPeriodId == null ? (Guid?)null : accountingPeriodId.Value,
                value => value == null ? null : new AccountId(value.Value));
    }
}
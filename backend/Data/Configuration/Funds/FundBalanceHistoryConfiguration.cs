using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Funds;

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

        builder.Property(accountBalanceHistory => accountBalanceHistory.TransactionId)
            .HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));

        builder.OwnsMany(fundBalanceHistory => fundBalanceHistory.AccountBalances, accountAmount =>
        {
            accountAmount.ToTable("FundBalanceHistoryAccountBalances");
            accountAmount.Property<int>("Id");
            accountAmount.HasKey("Id");

            accountAmount.Property(accountBalance => accountBalance.AccountId)
                .HasConversion(accountId => accountId.Value, value => new AccountId(value));
        });
        builder.Navigation(fundBalanceHistory => fundBalanceHistory.AccountBalances).AutoInclude();

        builder.OwnsMany(fundBalanceHistory => fundBalanceHistory.PendingDebits, accountAmount =>
        {
            accountAmount.ToTable("FundBalanceHistoryPendingDebits");
            accountAmount.Property<int>("Id");
            accountAmount.HasKey("Id");

            accountAmount.Property(accountBalance => accountBalance.AccountId)
                .HasConversion(accountId => accountId.Value, value => new AccountId(value));
        });
        builder.Navigation(fundBalanceHistory => fundBalanceHistory.PendingDebits).AutoInclude();

        builder.OwnsMany(fundBalanceHistory => fundBalanceHistory.PendingCredits, accountAmount =>
        {
            accountAmount.ToTable("FundBalanceHistoryPendingCredits");
            accountAmount.Property<int>("Id");
            accountAmount.HasKey("Id");

            accountAmount.Property(accountBalance => accountBalance.AccountId)
                .HasConversion(accountId => accountId.Value, value => new AccountId(value));
        });
        builder.Navigation(fundBalanceHistory => fundBalanceHistory.PendingCredits).AutoInclude();
    }
}
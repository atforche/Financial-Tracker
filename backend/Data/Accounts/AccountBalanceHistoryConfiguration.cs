using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Accounts;

/// <summary>
/// EF Core entity configuration for a <see cref="AccountBalanceHistory"/>
/// </summary>
internal sealed class AccountBalanceHistoryConfiguration : IEntityTypeConfiguration<AccountBalanceHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountBalanceHistory> builder)
    {
        builder.HasKey(accountBalanceHistory => accountBalanceHistory.Id);
        builder.Property(accountBalanceHistory => accountBalanceHistory.Id).HasConversion(accountBalanceHistoryId => accountBalanceHistoryId.Value, value => new AccountBalanceHistoryId(value));

        builder.HasOne(accountBalanceHistory => accountBalanceHistory.Account).WithMany();
        builder.Navigation(accountBalanceHistory => accountBalanceHistory.Account).AutoInclude();

        builder.Property(accountBalanceHistory => accountBalanceHistory.TransactionId)
            .HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));

        builder.OwnsMany(accountBalanceHistory => accountBalanceHistory.FundBalances, fundAmount =>
        {
            fundAmount.ToTable("AccountBalanceHistoryFundBalances");
            fundAmount.Property<int>("Id");
            fundAmount.HasKey("Id");

            fundAmount.Property(fundBalance => fundBalance.FundId)
                .HasConversion(fundId => fundId.Value, value => new FundId(value));
        });
        builder.Navigation(accountBalanceHistory => accountBalanceHistory.FundBalances).AutoInclude();

        builder.OwnsMany(accountBalanceHistory => accountBalanceHistory.PendingDebits, fundAmount =>
        {
            fundAmount.ToTable("AccountBalanceHistoryPendingDebits");
            fundAmount.Property<int>("Id");
            fundAmount.HasKey("Id");

            fundAmount.Property(fundBalance => fundBalance.FundId)
                .HasConversion(fundId => fundId.Value, value => new FundId(value));
        });
        builder.Navigation(accountBalanceHistory => accountBalanceHistory.PendingDebits).AutoInclude();

        builder.OwnsMany(accountBalanceHistory => accountBalanceHistory.PendingCredits, fundAmount =>
        {
            fundAmount.ToTable("AccountBalanceHistoryPendingCredits");
            fundAmount.Property<int>("Id");
            fundAmount.HasKey("Id");

            fundAmount.Property(fundBalance => fundBalance.FundId)
                .HasConversion(fundId => fundId.Value, value => new FundId(value));
        });
        builder.Navigation(accountBalanceHistory => accountBalanceHistory.PendingCredits).AutoInclude();
    }
}
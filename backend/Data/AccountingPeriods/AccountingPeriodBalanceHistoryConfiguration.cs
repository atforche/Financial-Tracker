using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.AccountingPeriods;

/// <summary>
/// EF Core entity configuration for an <see cref="AccountingPeriodBalanceHistory"/>
/// </summary>
internal sealed class AccountingPeriodBalanceHistoryConfiguration : IEntityTypeConfiguration<AccountingPeriodBalanceHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountingPeriodBalanceHistory> builder)
    {
        builder.HasKey(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.Id);
        builder.Property(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.Id)
            .HasConversion(accountingPeriodBalanceHistoryId => accountingPeriodBalanceHistoryId.Value, value => new AccountingPeriodBalanceHistoryId(value));

        builder.HasOne(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountingPeriod).WithMany();
        builder.Navigation(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountingPeriod).AutoInclude();

        builder.OwnsMany(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountBalances, builder =>
        {
            builder.WithOwner().HasForeignKey("AccountingPeriodBalanceHistoryId");

            builder.HasKey(accountBalance => accountBalance.Id);
            builder.Property(accountBalance => accountBalance.Id)
                .HasConversion(accountBalanceId => accountBalanceId.Value, value => new AccountAccountingPeriodBalanceHistoryId(value));

            builder.HasOne(accountBalance => accountBalance.Account).WithMany();
            builder.Navigation(accountBalance => accountBalance.Account).AutoInclude();

            builder.HasOne(accountBalance => accountBalance.AccountingPeriod).WithMany();
            builder.Navigation(accountBalance => accountBalance.AccountingPeriod).AutoInclude();

            builder.OwnsMany(accountBalance => accountBalance.OpeningFundBalances, fundAmount =>
            {
                fundAmount.ToTable("AccountAccountingPeriodBalanceHistoryOpeningFundBalances");
                fundAmount.Property<int>("Id");
                fundAmount.HasKey("Id");

                fundAmount.Property(fundBalance => fundBalance.FundId)
                    .HasConversion(fundId => fundId.Value, value => new FundId(value));
            });
            builder.Navigation(accountBalance => accountBalance.OpeningFundBalances).AutoInclude();

            builder.OwnsMany(accountBalance => accountBalance.ClosingFundBalances, fundAmount =>
            {
                fundAmount.ToTable("AccountAccountingPeriodBalanceHistoryClosingFundBalances");
                fundAmount.Property<int>("Id");
                fundAmount.HasKey("Id");

                fundAmount.Property(fundBalance => fundBalance.FundId)
                    .HasConversion(fundId => fundId.Value, value => new FundId(value));
            });
            builder.Navigation(accountBalance => accountBalance.ClosingFundBalances).AutoInclude();
        });
        builder.Navigation(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountBalances).AutoInclude();

        builder.OwnsMany(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.FundBalances, builder =>
        {
            builder.WithOwner().HasForeignKey("AccountingPeriodBalanceHistoryId");

            builder.HasKey(fundBalance => fundBalance.Id);
            builder.Property(fundBalance => fundBalance.Id)
                .HasConversion(fundBalanceId => fundBalanceId.Value, value => new FundAccountingPeriodBalanceHistoryId(value));

            builder.HasOne(fundBalance => fundBalance.Fund).WithMany();
            builder.Navigation(fundBalance => fundBalance.Fund).AutoInclude();

            builder.HasOne(fundBalance => fundBalance.AccountingPeriod).WithMany();
            builder.Navigation(fundBalance => fundBalance.AccountingPeriod).AutoInclude();

            builder.OwnsMany(fundBalance => fundBalance.OpeningAccountBalances, accountBalance =>
            {
                accountBalance.ToTable("FundAccountingPeriodBalanceHistoryOpeningAccountBalances");
                accountBalance.Property<int>("Id");
                accountBalance.HasKey("Id");

                accountBalance.Property(accountBalance => accountBalance.AccountId)
                    .HasConversion(accountId => accountId.Value, value => new AccountId(value));
            });
            builder.Navigation(fundBalance => fundBalance.OpeningAccountBalances).AutoInclude();

            builder.OwnsMany(fundBalance => fundBalance.ClosingAccountBalances, accountBalance =>
            {
                accountBalance.ToTable("FundAccountingPeriodBalanceHistoryClosingAccountBalances");
                accountBalance.Property<int>("Id");
                accountBalance.HasKey("Id");

                accountBalance.Property(accountBalance => accountBalance.AccountId)
                    .HasConversion(accountId => accountId.Value, value => new AccountId(value));
            });
            builder.Navigation(fundBalance => fundBalance.ClosingAccountBalances).AutoInclude();
        });
        builder.Navigation(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.FundBalances).AutoInclude();
    }
}
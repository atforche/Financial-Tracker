using Domain.AccountingPeriods;
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
                .HasConversion(accountBalanceId => accountBalanceId.Value, value => new AccountingPeriodAccountBalanceHistoryId(value));

            builder.HasOne(accountBalance => accountBalance.Account).WithMany();
            builder.Navigation(accountBalance => accountBalance.Account).AutoInclude();

            builder.HasOne(accountBalance => accountBalance.AccountingPeriod).WithMany();
            builder.Navigation(accountBalance => accountBalance.AccountingPeriod).AutoInclude();
        });
        builder.Navigation(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountBalances).AutoInclude();

        builder.OwnsMany(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.FundBalances, builder =>
        {
            builder.WithOwner().HasForeignKey("AccountingPeriodBalanceHistoryId");

            builder.HasKey(fundBalance => fundBalance.Id);
            builder.Property(fundBalance => fundBalance.Id)
                .HasConversion(fundBalanceId => fundBalanceId.Value, value => new AccountingPeriodFundBalanceHistoryId(value));

            builder.HasOne(fundBalance => fundBalance.Fund).WithMany();
            builder.Navigation(fundBalance => fundBalance.Fund).AutoInclude();

            builder.HasOne(fundBalance => fundBalance.AccountingPeriod).WithMany();
            builder.Navigation(fundBalance => fundBalance.AccountingPeriod).AutoInclude();
        });
        builder.Navigation(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.FundBalances).AutoInclude();
    }
}
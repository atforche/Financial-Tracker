using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.AccountingPeriods;

/// <summary>
/// EF Core entity configuration for an <see cref="AccountingPeriod"/>
/// </summary>
internal sealed class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.HasKey(accountingPeriod => accountingPeriod.Id);
        builder.Property(accountingPeriod => accountingPeriod.Id)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasMany(accountingPeriod => accountingPeriod.Transactions)
            .WithOne(transaction => transaction.AccountingPeriod)
            .HasForeignKey("AccountingPeriodId");
        builder.Navigation(accountingPeriod => accountingPeriod.Transactions).AutoInclude();

        builder.HasMany(accountingPeriod => accountingPeriod.FundConversions)
            .WithOne()
            .HasForeignKey(fundConversion => fundConversion.AccountingPeriodId);
        builder.Navigation(accountingPeriod => accountingPeriod.FundConversions).AutoInclude();

        builder.HasMany(accountingPeriod => accountingPeriod.ChangeInValues)
            .WithOne()
            .HasForeignKey(changeInValue => changeInValue.AccountingPeriodId);
        builder.Navigation(accountingPeriod => accountingPeriod.ChangeInValues).AutoInclude();
    }
}
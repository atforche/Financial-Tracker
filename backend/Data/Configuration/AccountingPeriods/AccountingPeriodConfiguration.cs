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

        builder.HasMany(accountingPeriod => accountingPeriod.ChangeInValues)
            .WithOne()
            .HasForeignKey(changeInValue => changeInValue.AccountingPeriodId);
        builder.Navigation(accountingPeriod => accountingPeriod.ChangeInValues).AutoInclude();
    }
}
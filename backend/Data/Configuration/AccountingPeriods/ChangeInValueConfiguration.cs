using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.AccountingPeriods;

/// <summary>
/// EF Core entity configuration for a <see cref="ChangeInValue"/>
/// </summary>
internal sealed class ChangeInValueConfiguration : EntityConfiguration<ChangeInValue>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<ChangeInValue> builder)
    {
        builder.Property(changeInValue => changeInValue.AccountingPeriodId)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasIndex(changeInValue => new { changeInValue.EventDate, changeInValue.EventSequence }).IsUnique();

        builder.HasOne(changeInValue => changeInValue.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(changeInValue => changeInValue.Account).IsRequired().AutoInclude();

        builder.HasOne(changeInValue => changeInValue.AccountingEntry).WithOne().HasForeignKey<ChangeInValue>("FundAmountId");
        builder.Navigation(changeInValue => changeInValue.AccountingEntry).IsRequired().AutoInclude();
    }
}
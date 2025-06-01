using Domain.AccountingPeriods;
using Domain.Accounts;
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
        builder.HasIndex(changeInValue => new { changeInValue.EventDate, changeInValue.EventSequence }).IsUnique();

        builder.HasOne<Account>().WithMany().HasForeignKey(changeInValue => changeInValue.AccountId);

        builder.HasOne(changeInValue => changeInValue.FundAmount).WithOne().HasForeignKey<ChangeInValue>("FundAmountId");
        builder.Navigation(changeInValue => changeInValue.FundAmount).IsRequired().AutoInclude();
    }
}
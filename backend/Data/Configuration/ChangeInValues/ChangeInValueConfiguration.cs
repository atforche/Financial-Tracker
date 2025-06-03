using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.ChangeInValues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.ChangeInValues;

/// <summary>
/// EF Core entity configuration for a <see cref="ChangeInValue"/>
/// </summary>
internal sealed class ChangeInValueConfiguration : IEntityTypeConfiguration<ChangeInValue>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ChangeInValue> builder)
    {
        builder.HasKey(changeInValue => changeInValue.Id);
        builder.Property(changeInValue => changeInValue.Id).HasConversion(changeInValueId => changeInValueId.Value, value => new ChangeInValueId(value));

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(changeInValue => changeInValue.AccountingPeriodId);

        builder.HasIndex(changeInValue => new { changeInValue.EventDate, changeInValue.EventSequence }).IsUnique();

        builder.HasOne<Account>().WithMany().HasForeignKey(changeInValue => changeInValue.AccountId);

        builder.HasOne(changeInValue => changeInValue.FundAmount).WithOne().HasForeignKey<ChangeInValue>("FundAmountId");
        builder.Navigation(changeInValue => changeInValue.FundAmount).IsRequired().AutoInclude();
    }
}
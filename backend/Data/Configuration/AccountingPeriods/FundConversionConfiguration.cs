using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.AccountingPeriods;

/// <summary>
/// EF Core entity configuration for a <see cref="FundConversion"/>
/// </summary>
internal sealed class FundConversionConfiguration : EntityConfiguration<FundConversion>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<FundConversion> builder)
    {
        builder.Property(fundConversion => fundConversion.AccountingPeriodId)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasIndex(fundConversion => new { fundConversion.EventDate, fundConversion.EventSequence }).IsUnique();

        builder.HasOne(fundConversion => fundConversion.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(fundConversion => fundConversion.Account).IsRequired().AutoInclude();

        builder.HasOne(fundConversion => fundConversion.FromFund).WithMany().HasForeignKey("FromFundId");
        builder.Navigation(fundConversion => fundConversion.FromFund).IsRequired().AutoInclude();

        builder.HasOne(fundConversion => fundConversion.ToFund).WithMany().HasForeignKey("ToFundId");
        builder.Navigation(fundConversion => fundConversion.ToFund).IsRequired().AutoInclude();
    }
}
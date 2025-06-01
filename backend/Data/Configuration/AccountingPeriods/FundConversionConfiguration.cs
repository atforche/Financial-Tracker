using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
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

        builder.HasOne<Account>().WithMany().HasForeignKey(fundConversion => fundConversion.AccountId);

        builder.HasOne<Fund>().WithMany().HasForeignKey(fundConversion => fundConversion.FromFundId);

        builder.HasOne<Fund>().WithMany().HasForeignKey(fundConversion => fundConversion.ToFundId);
    }
}
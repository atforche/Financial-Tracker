using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundConversions;
using Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.FundConversions;

/// <summary>
/// EF Core entity configuration for a <see cref="FundConversion"/>
/// </summary>
internal sealed class FundConversionConfiguration : IEntityTypeConfiguration<FundConversion>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundConversion> builder)
    {
        builder.HasKey(fundConversion => fundConversion.Id);
        builder.Property(fundConversion => fundConversion.Id).HasConversion(fundConversionId => fundConversionId.Value, value => new FundConversionId(value));

        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(fundConversion => fundConversion.AccountingPeriodId);

        builder.HasIndex(fundConversion => new { fundConversion.EventDate, fundConversion.EventSequence }).IsUnique();

        builder.HasOne<Account>().WithMany().HasForeignKey(fundConversion => fundConversion.AccountId);

        builder.HasOne<Fund>().WithMany().HasForeignKey(fundConversion => fundConversion.FromFundId);

        builder.HasOne<Fund>().WithMany().HasForeignKey(fundConversion => fundConversion.ToFundId);
    }
}
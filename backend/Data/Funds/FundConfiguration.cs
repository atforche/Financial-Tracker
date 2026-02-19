using Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Funds;

/// <summary>
/// EF Core entity configuration for a <see cref="Fund"/>
/// </summary>
internal sealed class FundConfiguration : IEntityTypeConfiguration<Fund>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Fund> builder)
    {
        builder.HasKey(fund => fund.Id);
        builder.Property(fund => fund.Id).HasConversion(fundId => fundId.Value, value => new FundId(value));

        builder.HasIndex(fund => fund.Name).IsUnique();
    }
}
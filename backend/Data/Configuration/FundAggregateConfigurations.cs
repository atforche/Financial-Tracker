using Domain.Aggregates.Funds;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration;

/// <summary>
/// EF Core configuration for the Fund entity
/// </summary>
internal sealed class FundEntityConfiguration : EntityConfiguration<Fund>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<Fund> builder) =>
        builder.HasIndex(fund => fund.Name).IsUnique();
}
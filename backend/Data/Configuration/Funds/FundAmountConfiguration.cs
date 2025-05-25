using Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Funds;

/// <summary>
/// EF Core entity configuration for a <see cref="FundAmount"/>
/// </summary>
internal sealed class FundAmountConfiguration : IEntityTypeConfiguration<FundAmount>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundAmount> builder)
    {
        // Use an auto-incrementing shadow property key since a Fund Amount is a value object
        builder.Property<int>("Id");
        builder.HasKey("Id");

        builder.HasOne<Fund>().WithMany().HasForeignKey(fundAmount => fundAmount.FundId);
    }
}
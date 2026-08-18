using Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Locations;

/// <summary>
/// EF Core configuration for a <see cref="Location"/>.
/// </summary>
internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(location => location.Id);
        builder.Property(location => location.Id).HasConversion(locationId => locationId.Value, value => new LocationId(value));
        builder.HasIndex(location => location.NormalizedName).IsUnique();
    }
}

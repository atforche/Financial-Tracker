using Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration;

/// <summary>
/// Base Entity Type Configuation for an Entity type
/// </summary>
internal abstract class EntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : EntityBase
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property("_internalId").HasColumnName("InternalId");
        builder.HasKey("_internalId");

        builder.Property("_externalId").HasColumnName("ExternalId");
        builder.HasIndex("_externalId").IsUnique();

        ConfigurePrivate(builder);
    }

    /// <summary>
    /// Applies additional configurations to the provided Entity Type Builder
    /// </summary>
    /// <param name="builder">Entity Type Builder for the current type</param>
    protected abstract void ConfigurePrivate(EntityTypeBuilder<TEntity> builder);
}
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration;

/// <summary>
/// Base Entity Type Configuration for an Entity type
/// </summary>
internal abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : EntityOld
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasConversion(entityId => entityId.Value, value => new EntityId(value));

        ConfigurePrivate(builder);
    }

    /// <summary>
    /// Applies additional configurations to the provided Entity Type Builder
    /// </summary>
    /// <param name="builder">Entity Type Builder for the current type</param>
    protected abstract void ConfigurePrivate(EntityTypeBuilder<TEntity> builder);
}
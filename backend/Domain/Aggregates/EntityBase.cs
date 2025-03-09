using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// Base class shared by all Entities
/// </summary>
public abstract class EntityBase(EntityId id) : IEquatable<EntityBase>
{
    /// <summary>
    /// ID for this Entity
    /// </summary>
    public EntityId Id { get; } = id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EntityBase other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(EntityBase? other) => other != null && other.Id == Id;

    /// <inheritdoc/>
    public static bool operator ==(EntityBase? first, EntityBase? second)
    {
        if (first is null)
        {
            return second is null;
        }
        return first.Equals(second);
    }

    /// <inheritdoc/>
    public static bool operator !=(EntityBase? first, EntityBase? second) => !(first == second);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();
}
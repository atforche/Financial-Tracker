namespace Domain;

/// <summary>
/// Base class shared by all Entities
/// </summary>
public abstract class EntityOld : IEquatable<EntityOld>
{
    /// <summary>
    /// ID for this Entity
    /// </summary>
    public EntityId Id { get; } = new EntityId(Guid.NewGuid());

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EntityOld other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(EntityOld? other) => other != null && other.Id == Id;

    /// <inheritdoc/>
    public static bool operator ==(EntityOld? first, EntityOld? second)
    {
        if (first is null)
        {
            return second is null;
        }
        return first.Equals(second);
    }

    /// <inheritdoc/>
    public static bool operator !=(EntityOld? first, EntityOld? second) => !(first == second);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();
}
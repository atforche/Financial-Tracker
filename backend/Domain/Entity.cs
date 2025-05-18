namespace Domain;

/// <summary>
/// Base class shared by all Entities
/// </summary>
public abstract class Entity(EntityId id) : IEquatable<Entity>
{
    private readonly long _internalId = id.InternalId;
    private readonly Guid _externalId = id.ExternalId;

    /// <summary>
    /// ID for this Entity
    /// </summary>
    public EntityId Id => new(_internalId, _externalId);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Entity other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Entity? other) => other != null && other.Id == Id;

    /// <inheritdoc/>
    public static bool operator ==(Entity? first, Entity? second)
    {
        if (first is null)
        {
            return second is null;
        }
        return first.Equals(second);
    }

    /// <inheritdoc/>
    public static bool operator !=(Entity? first, Entity? second) => !(first == second);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();
}
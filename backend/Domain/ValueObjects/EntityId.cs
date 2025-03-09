namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing the ID of an Entity
/// </summary>
public class EntityId : IEquatable<EntityId>
{
    /// <summary>
    /// Internal ID for this Entity
    /// </summary>
    public long InternalId { get; }

    /// <summary>
    /// External ID for this Entity
    /// </summary>
    public Guid ExternalId { get; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EntityId other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(EntityId? other)
    {
        if (other == null)
        {
            return false;
        }
        if (InternalId == default || other.InternalId == default)
        {
            return ExternalId == other.ExternalId;
        }
        return InternalId == other.InternalId;
    }

    /// <inheritdoc/>
    public static bool operator ==(EntityId? first, EntityId? second)
    {
        if (first is null)
        {
            return second is null;
        }
        return first.Equals(second);
    }

    /// <inheritdoc/>
    public static bool operator !=(EntityId? first, EntityId? second) => !(first == second);

    /// <inheritdoc/>
    public override int GetHashCode() => InternalId.GetHashCode();

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="internalId">Internal ID for this Entity</param>
    /// <param name="externalId">External ID for this Entity</param>
    internal EntityId(long internalId, Guid externalId)
    {
        InternalId = internalId;
        ExternalId = externalId;
        Validate();
    }

    /// <summary>
    /// Validates this Entity ID
    /// </summary>
    private void Validate()
    {
        if (ExternalId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
    }
}
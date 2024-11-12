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

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="internalId">Internal ID for this Entity</param>
    /// <param name="externalId">External ID for this Entity</param>
    public EntityId(long internalId, Guid externalId)
    {
        InternalId = internalId;
        ExternalId = externalId;
        Validate();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EntityId other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(EntityId? other)
    {
        if (other == null)
        {
            return false;
        }
        if (InternalId == default(long) || other.InternalId == default(long))
        {
            return ExternalId == other.ExternalId;
        }
        return InternalId == other.InternalId;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => InternalId.GetHashCode();

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
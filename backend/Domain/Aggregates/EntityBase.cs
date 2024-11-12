using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// Base class shared by all Entities
/// </summary>
public abstract class EntityBase : IEquatable<EntityBase>
{
    private readonly long _internalId;
    private readonly Guid _externalId;

    /// <summary>
    /// ID for this Entity
    /// </summary>
    public EntityId Id => new EntityId(_internalId, _externalId);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EntityBase other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(EntityBase? other) => other != null && other.Id == Id;

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">ID for this Entity</param>
    protected EntityBase(EntityId id)
    {
        _internalId = id.InternalId;
        _externalId = id.ExternalId;
    }
}
namespace Domain;

/// <summary>
/// Base class shared by all Entities
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : EntityId
{
    /// <summary>
    /// ID for this Entity
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">ID for this entity</param>
    protected Entity(TId id) => Id = id;

    /// <summary>
    /// Constructs a default instance of this class
    /// </summary>
    protected Entity() => Id = null!;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Entity<TId>? other) => other != null && other.Id == Id;

    /// <inheritdoc/>
    public static bool operator ==(Entity<TId>? first, Entity<TId>? second)
    {
        if (first is null)
        {
            return second is null;
        }
        return first.Equals(second);
    }

    /// <inheritdoc/>
    public static bool operator !=(Entity<TId>? first, Entity<TId>? second) => !(first == second);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();
}
namespace Domain;

/// <summary>
/// Value object class representing the ID of an Entity
/// </summary>
public class EntityId : IEquatable<EntityId>
{
    /// <summary>
    /// Value for this Entity ID
    /// </summary>
    public Guid Value { get; }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EntityId other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(EntityId? other)
    {
        if (other == null)
        {
            return false;
        }
        return Value == other.Value;
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
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="value">Value for this Entity ID</param>
    public EntityId(Guid value)
    {
        Value = value;
        Validate();
    }

    /// <summary>
    /// Validates this Entity ID
    /// </summary>
    private void Validate()
    {
        if (Value == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
    }
}
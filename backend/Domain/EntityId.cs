namespace Domain;

/// <summary>
/// Value object class representing the ID of an Entity
/// </summary>
public record EntityId : IComparable<EntityId>, IComparable
{
    /// <summary>
    /// Value for this Entity ID
    /// </summary>
    public Guid Value { get; }

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

    /// <summary>
    /// Compares this EntityId to another EntityId by its underlying Guid value.
    /// </summary>
    public int CompareTo(EntityId? other) => other is null ? 1 : Value.CompareTo(other.Value);

    /// <summary>
    /// Compares this EntityId to another object.
    /// </summary>
    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        EntityId other => CompareTo(other),
        _ => throw new ArgumentException("Object must be of type EntityId.", nameof(obj)),
    };

    /// <inheritdoc/>
    public static bool operator <(EntityId left, EntityId right) => left is null ? right is not null : left.CompareTo(right) < 0;

    /// <inheritdoc/>
    public static bool operator <=(EntityId left, EntityId right) => left is null || left.CompareTo(right) <= 0;

    /// <inheritdoc/>
    public static bool operator >(EntityId left, EntityId right) => left is not null && left.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static bool operator >=(EntityId left, EntityId right) => left is null ? right is null : left.CompareTo(right) >= 0;

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected EntityId()
    {
    }
}
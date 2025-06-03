namespace Domain;

/// <summary>
/// Value object class representing the ID of an Entity
/// </summary>
public record EntityId
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
}
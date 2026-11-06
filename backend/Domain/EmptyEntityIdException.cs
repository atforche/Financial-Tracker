namespace Domain;

/// <summary>
/// Exception thrown when an Entity ID is created from an empty value, which can't identify anything.
/// </summary>
public sealed class EmptyEntityIdException : InvalidOperationException
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public EmptyEntityIdException()
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public EmptyEntityIdException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public EmptyEntityIdException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

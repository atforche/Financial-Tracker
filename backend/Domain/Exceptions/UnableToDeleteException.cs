namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided entity cannot be deleted.
/// </summary>
public class UnableToDeleteException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToDeleteException() : base("The provided entity cannot be deleted.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToDeleteException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToDeleteException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
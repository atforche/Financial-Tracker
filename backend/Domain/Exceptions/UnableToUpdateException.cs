namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided entity cannot be updated.
/// </summary>
public class UnableToUpdateException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToUpdateException() : base("The provided entity cannot be updated.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToUpdateException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToUpdateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
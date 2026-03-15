namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided accounting period cannot be closed.
/// </summary>
public class UnableToCloseException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToCloseException() : base("The provided accounting period cannot be closed.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToCloseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToCloseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
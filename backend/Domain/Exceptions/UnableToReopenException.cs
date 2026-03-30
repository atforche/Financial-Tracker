namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided accounting period cannot be reopened.
/// </summary>
public class UnableToReopenException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToReopenException() : base("The provided accounting period cannot be reopened.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToReopenException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToReopenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
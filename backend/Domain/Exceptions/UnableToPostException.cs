namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a transaction cannot be posted to an account.
/// </summary>
public class UnableToPostException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToPostException() : base("The provided transaction cannot be posted.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToPostException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToPostException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a transaction cannot be unposted from an account.
/// </summary>
public class UnableToUnpostException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToUnpostException() : base("The provided transaction cannot be unposted.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToUnpostException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToUnpostException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
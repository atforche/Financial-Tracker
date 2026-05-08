namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided account is invalid.
/// </summary>
public class InvalidAccountException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountException() : base("The provided account is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
namespace Domain.Accounts.Exceptions;

/// <summary>
/// Exception thrown when an Account name is invalid
/// </summary>
public class InvalidAccountNameException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountNameException() : base("The provided Account name is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidAccountNameException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidAccountNameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
namespace Domain.Accounts.Exceptions;

/// <summary>
/// Exception thrown when an Account's add date is invalid
/// </summary>
public class InvalidAddDateException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAddDateException() : base("The provided add date is invalid for the Account.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidAddDateException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidAddDateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
namespace Domain.Transactions.Exceptions;

/// <summary>
/// Exception thrown when a Transaction has an invalid Account
/// </summary>
public class InvalidAccountException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountException() : base("The Transaction has an invalid Account.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidAccountException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidAccountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
namespace Domain.BalanceEvents.Exceptions;

/// <summary>
/// Exception thrown a Balance Event would result in an invalid Account balance
/// </summary>
public class InvalidAccountBalanceException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountBalanceException() : base("This balance event would result in an invalid account balance.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidAccountBalanceException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidAccountBalanceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
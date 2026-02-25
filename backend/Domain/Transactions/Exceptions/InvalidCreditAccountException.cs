namespace Domain.Transactions.Exceptions;

/// <summary>
/// Exception thrown when a Transaction has an invalid credit Account
/// </summary>
public class InvalidCreditAccountException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidCreditAccountException() : base("The Transaction has an invalid credit Account.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidCreditAccountException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidCreditAccountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
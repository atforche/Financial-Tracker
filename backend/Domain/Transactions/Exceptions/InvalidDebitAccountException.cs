namespace Domain.Transactions.Exceptions;

/// <summary>
/// Exception thrown when a Transaction has an invalid debit Account
/// </summary>
public class InvalidDebitAccountException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidDebitAccountException() : base("The Transaction has an invalid debit Account.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidDebitAccountException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidDebitAccountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
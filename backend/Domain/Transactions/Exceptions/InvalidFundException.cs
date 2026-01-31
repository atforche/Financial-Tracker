namespace Domain.Transactions.Exceptions;

/// <summary>
/// Exception thrown when a Transaction has an invalid Fund
/// </summary>
public class InvalidFundException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundException() : base("The Transaction has an invalid Fund.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidFundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidFundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
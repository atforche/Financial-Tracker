namespace Domain.Transactions.Exceptions;

/// <summary>
/// Exception thrown when a Transaction cannot be updated
/// </summary>
public class UnableToUpdateException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToUpdateException() : base("The Transaction cannot be updated.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public UnableToUpdateException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public UnableToUpdateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
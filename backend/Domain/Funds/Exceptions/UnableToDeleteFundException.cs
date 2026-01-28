namespace Domain.Funds.Exceptions;

/// <summary>
/// Exception thrown when a Fund cannot be deleted
/// </summary>
public class UnableToDeleteFundException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToDeleteFundException() : base("Unable to delete this Fund.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public UnableToDeleteFundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public UnableToDeleteFundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
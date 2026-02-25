namespace Domain.Funds.Exceptions;

/// <summary>
/// Exception thrown when a Fund cannot be deleted
/// </summary>
public class UnableToDeleteException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToDeleteException() : base("Unable to delete this Fund.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public UnableToDeleteException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public UnableToDeleteException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
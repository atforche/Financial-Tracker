namespace Domain.BalanceEvents.Exceptions;

/// <summary>
/// Exception thrown when a Balance Event has an invalid Event Date
/// </summary>
public class InvalidEventDateException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidEventDateException() : base("The Balance Event has an invalid Event Date.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidEventDateException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidEventDateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
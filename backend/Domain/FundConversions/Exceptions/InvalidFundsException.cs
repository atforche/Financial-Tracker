namespace Domain.FundConversions.Exceptions;

/// <summary>
/// Exception thrown when a Fund Conversation has invalid funds
/// </summary>
public class InvalidFundsException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundsException() : base("A Fund Conversation must have different from and to funds.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidFundsException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidFundsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
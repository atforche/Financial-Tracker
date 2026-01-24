namespace Domain.FundConversions.Exceptions;

/// <summary>
/// Exception thrown when a Fund Conversion has an invalid amount
/// </summary>
public class InvalidAmountException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAmountException() : base("The provided amount is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidAmountException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidAmountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
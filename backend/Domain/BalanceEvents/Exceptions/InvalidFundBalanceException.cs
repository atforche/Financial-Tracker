namespace Domain.BalanceEvents.Exceptions;

/// <summary>
/// Exception thrown a Balance Event would result in an invalid Fund balance
/// </summary>
public class InvalidFundBalanceException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundBalanceException() : base("This balance event would result in an invalid fund balance.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidFundBalanceException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidFundBalanceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
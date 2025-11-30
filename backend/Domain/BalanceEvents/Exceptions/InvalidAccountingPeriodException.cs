namespace Domain.BalanceEvents.Exceptions;

/// <summary>
/// Exception thrown when a Balance Event has an invalid Accounting Period
/// </summary>
public class InvalidAccountingPeriodException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountingPeriodException() : base("The Balance Event has an invalid Accounting Period.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidAccountingPeriodException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidAccountingPeriodException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
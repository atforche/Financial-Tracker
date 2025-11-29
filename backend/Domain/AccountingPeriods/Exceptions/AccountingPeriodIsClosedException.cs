namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when attempting to do something with an already closed Accounting Period
/// </summary>
public class AccountingPeriodIsClosedException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public AccountingPeriodIsClosedException() : base("This Accounting Period is closed.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public AccountingPeriodIsClosedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public AccountingPeriodIsClosedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when we're unable to close an Accounting Period
/// </summary>
public class UnableToCloseAccountingPeriodException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToCloseAccountingPeriodException() : base("Unable to close this Accounting Period.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public UnableToCloseAccountingPeriodException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public UnableToCloseAccountingPeriodException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
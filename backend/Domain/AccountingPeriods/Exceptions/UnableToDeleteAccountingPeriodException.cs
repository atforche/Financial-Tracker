namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when we're unable to delete an Accounting Period
/// </summary>
public class UnableToDeleteAccountingPeriodException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public UnableToDeleteAccountingPeriodException() : base("Unable to delete this Accounting Period.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public UnableToDeleteAccountingPeriodException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public UnableToDeleteAccountingPeriodException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
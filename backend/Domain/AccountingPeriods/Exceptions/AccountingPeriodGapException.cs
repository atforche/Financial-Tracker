namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when there is a gap between Accounting Periods
/// </summary>
public class AccountingPeriodGapException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public AccountingPeriodGapException() : base("This action would cause a gap between Accounting Periods.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public AccountingPeriodGapException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public AccountingPeriodGapException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
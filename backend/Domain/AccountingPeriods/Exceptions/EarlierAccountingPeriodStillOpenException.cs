namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when attempting to close an Accounting Period while an earlier period is still open
/// </summary>
public class EarlierAccountingPeriodStillOpenException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public EarlierAccountingPeriodStillOpenException() : base("An earlier Accounting Period is still open.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public EarlierAccountingPeriodStillOpenException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public EarlierAccountingPeriodStillOpenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
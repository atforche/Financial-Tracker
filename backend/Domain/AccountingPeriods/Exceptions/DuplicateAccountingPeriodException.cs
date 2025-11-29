namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when a duplicate Accounting Period is being created
/// </summary>
public class DuplicateAccountingPeriodException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public DuplicateAccountingPeriodException() : base("This Accounting Period already exists.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public DuplicateAccountingPeriodException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public DuplicateAccountingPeriodException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
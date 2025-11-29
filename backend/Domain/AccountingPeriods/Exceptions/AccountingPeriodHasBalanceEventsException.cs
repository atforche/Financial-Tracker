namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when attempting to delete an Accounting Period that has associated balance events
/// </summary>
public class AccountingPeriodHasBalanceEventsException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public AccountingPeriodHasBalanceEventsException() : base("This Accounting Period still has associated balance events.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public AccountingPeriodHasBalanceEventsException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public AccountingPeriodHasBalanceEventsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
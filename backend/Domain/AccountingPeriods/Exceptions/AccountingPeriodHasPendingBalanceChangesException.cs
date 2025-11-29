namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when attempting to close an Accounting Period that still has pending balance changes
/// </summary>
public class AccountingPeriodHasPendingBalanceChangesException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public AccountingPeriodHasPendingBalanceChangesException() : base("This Accounting Period still has pending balance changes.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public AccountingPeriodHasPendingBalanceChangesException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public AccountingPeriodHasPendingBalanceChangesException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
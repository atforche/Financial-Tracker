namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when adding an Accounting Period that doesn't fall directly after the most recent existing Accounting Period
/// </summary>
public class NonSequentialAccountingPeriodException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public NonSequentialAccountingPeriodException() : base("New Accounting Period must directly follow the most recent existing Accounting Period.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public NonSequentialAccountingPeriodException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public NonSequentialAccountingPeriodException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
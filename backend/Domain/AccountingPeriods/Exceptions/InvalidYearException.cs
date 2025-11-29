namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when an invalid year is provided for an Accounting Period
/// </summary>
public class InvalidYearException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidYearException() : base("The year provided is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidYearException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidYearException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
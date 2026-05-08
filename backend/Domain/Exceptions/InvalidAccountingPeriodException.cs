namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided accounting period is invalid.
/// </summary>
public class InvalidAccountingPeriodException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountingPeriodException() : base("The provided accounting period is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountingPeriodException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountingPeriodException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
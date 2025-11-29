namespace Domain.AccountingPeriods.Exceptions;

/// <summary>
/// Exception thrown when an invalid month is provided for an Accounting Period
/// </summary>
public class InvalidMonthException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidMonthException() : base("The month provided is invalid. It must be between 1 and 12.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidMonthException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidMonthException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
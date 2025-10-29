namespace Domain.Funds.Exceptions;

/// <summary>
/// Exception thrown when a Fund is still in use and cannot be deleted
/// </summary>
public class FundStillInUseException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public FundStillInUseException() : base("The Fund is still in use and cannot be deleted.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public FundStillInUseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public FundStillInUseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
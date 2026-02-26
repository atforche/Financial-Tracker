namespace Domain.Funds.Exceptions;

/// <summary>
/// Exception thrown when a Fund name is invalid
/// </summary>
public class InvalidNameException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidNameException() : base("The provided Fund name is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidNameException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidNameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
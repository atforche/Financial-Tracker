namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided fund is invalid.
/// </summary>
public class InvalidFundException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundException() : base("The provided fund is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
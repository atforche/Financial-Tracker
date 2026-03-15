namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided fund amount is invalid.
/// </summary>
public class InvalidFundAmountException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundAmountException() : base("The provided fund amount is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundAmountException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundAmountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
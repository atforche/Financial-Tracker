namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided amount is invalid.
/// </summary>
public class InvalidAmountException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAmountException() : base("The provided amount is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAmountException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAmountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
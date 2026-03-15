namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided date is invalid.
/// </summary>
public class InvalidDateException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidDateException() : base("The provided date is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidDateException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidDateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided year is invalid.
/// </summary>
public class InvalidYearException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidYearException() : base("The provided year is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidYearException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidYearException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
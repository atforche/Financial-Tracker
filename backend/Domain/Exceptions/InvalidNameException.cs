namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided name is invalid.
/// </summary>
public class InvalidNameException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidNameException() : base("The provided name is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidNameException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidNameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided fund type is invalid.
/// </summary>
public class InvalidFundTypeException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundTypeException() : base("The provided fund type is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundTypeException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
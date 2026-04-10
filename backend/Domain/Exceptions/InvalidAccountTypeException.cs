namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided account type is invalid.
/// </summary>
public class InvalidAccountTypeException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountTypeException() : base("The provided account type is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountTypeException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidAccountTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
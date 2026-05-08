namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided goal type is invalid.
/// </summary>
public class InvalidGoalTypeException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidGoalTypeException() : base("The provided goal type is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidGoalTypeException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidGoalTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
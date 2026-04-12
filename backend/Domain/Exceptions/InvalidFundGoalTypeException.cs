namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided fund goal type is invalid.
/// </summary>
public class InvalidFundGoalTypeException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundGoalTypeException() : base("The provided fund goal type is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundGoalTypeException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundGoalTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
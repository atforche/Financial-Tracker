namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided budget is invalid.
/// </summary>
public class InvalidBudgetException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidBudgetException() : base("The provided budget is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidBudgetException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidBudgetException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

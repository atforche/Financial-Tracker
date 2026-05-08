namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when the provided month is invalid.
/// </summary>
public class InvalidMonthException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidMonthException() : base("The provided month is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidMonthException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidMonthException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
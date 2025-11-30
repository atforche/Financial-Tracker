namespace Domain.Transactions.Exceptions;

/// <summary>
/// Exception thrown when a Transaction has an invalid balance event part
/// </summary>
public class InvalidBalanceEventPartException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidBalanceEventPartException() : base("The provided balance event part is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidBalanceEventPartException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidBalanceEventPartException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
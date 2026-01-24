namespace Domain.Accounts.Exceptions;

/// <summary>
/// Exception thrown when an Account is added with an invalid Fund amount
/// </summary>
public class InvalidFundAmountException : Exception
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public InvalidFundAmountException() : base("The provided Fund amount is invalid.")
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    public InvalidFundAmountException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="message">Message for this Exception</param>
    /// <param name="innerException">Inner Exception</param>
    public InvalidFundAmountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
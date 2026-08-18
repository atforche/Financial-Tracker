namespace Domain.Transactions.Income;

/// <summary>
/// Individual income line for an income transaction.
/// </summary>
public class IncomeLine(string description, decimal amount)
{
    /// <summary>
    /// Description for this income line.
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// Amount for this income line.
    /// </summary>
    public decimal Amount { get; private set; } = amount;

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private IncomeLine() : this("", 0)
    {
    }
}

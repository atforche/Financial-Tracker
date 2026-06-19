namespace Domain.Transactions.Income;

/// <summary>
/// Individual income deduction for an income transaction.
/// </summary>
public class IncomeDeduction(string description, decimal amount)
{
    /// <summary>
    /// Description for this income deduction.
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// Amount for this income deduction.
    /// </summary>
    public decimal Amount { get; private set; } = amount;

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private IncomeDeduction() : this("", 0)
    {
    }
}
namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing an Accounting Entry
/// </summary>
public class AccountingEntry
{
    /// <summary>
    /// Amount for this Accounting Entry
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="amount">Amount for this Accounting Entry</param>
    public AccountingEntry(decimal amount)
    {
        Amount = amount;
        Validate();
    }

    /// <summary>
    /// Validates the current Accounting Entry
    /// </summary>
    private void Validate()
    {
        if (Amount <= 0)
        {
            throw new InvalidOperationException();
        }
    }
}
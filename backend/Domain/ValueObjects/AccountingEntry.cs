namespace Domain.ValueObjects;

/// <summary>
/// Class representing an Accounting Entry value object
/// </summary>
public class AccountingEntry
{
    /// <summary>
    /// Amount for this AccountingEntry
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="amount">Amount for this AccountingEntry</param>
    public AccountingEntry(decimal amount)
    {
        Amount = amount;
        Validate();
    }

    /// <summary>
    /// Validates the current AccountingEntry
    /// </summary>
    private void Validate()
    {
        if (Amount <= 0)
        {
            throw new InvalidOperationException();
        }
    }
}
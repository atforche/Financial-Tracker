namespace Domain.AccountingPeriods;

/// <summary>
/// A portion of expected net income transferred to an untracked account.
/// </summary>
public sealed class ExpectedUntrackedIncomeTransfer(string description, decimal amount)
{
    /// <summary>
    /// Description of the transfer.
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// Expected transfer amount for each payment.
    /// </summary>
    public decimal Amount { get; private set; } = amount;

    /// <summary>
    /// Constructs a default instance for Entity Framework.
    /// </summary>
    private ExpectedUntrackedIncomeTransfer() : this("", 0) { }
}

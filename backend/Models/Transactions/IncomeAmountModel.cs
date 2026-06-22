namespace Models.Transactions;

/// <summary>
/// Model representing an income amount.
/// </summary>
public class IncomeAmountModel
{
    /// <summary>
    /// The total amount of income.
    /// </summary>
    public required decimal Total { get; init; }

    /// <summary>
    /// The tracked portion of the income.
    /// </summary>
    public required decimal Tracked { get; init; }

    /// <summary>
    /// The untracked portion of the income.
    /// </summary>
    public required decimal Untracked { get; init; }
}
namespace Models.Transactions.Types;

/// <summary>
/// Model representing an income line on an income transaction.
/// </summary>
public sealed class IncomeLineModel
{
    /// <summary>
    /// Description for the income line.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the income line.
    /// </summary>
    public required decimal Amount { get; init; }
}

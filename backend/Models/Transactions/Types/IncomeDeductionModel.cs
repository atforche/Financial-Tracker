namespace Models.Transactions.Types;

/// <summary>
/// Model representing an income deduction on an income transaction.
/// </summary>
public sealed class IncomeDeductionModel
{
    /// <summary>
    /// Description for the income deduction.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the income deduction.
    /// </summary>
    public required decimal Amount { get; init; }
}
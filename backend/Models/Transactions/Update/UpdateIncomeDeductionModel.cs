namespace Models.Transactions.Update;

/// <summary>
/// Model representing an income deduction in an update income transaction request.
/// </summary>
public sealed class UpdateIncomeDeductionModel
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
namespace Models.Transactions.Create;

/// <summary>
/// Model representing an income deduction in a create income transaction request.
/// </summary>
public sealed class CreateIncomeDeductionModel
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

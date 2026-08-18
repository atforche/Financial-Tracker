namespace Models.Transactions.Create;

/// <summary>
/// Model representing an income line in a create income transaction request.
/// </summary>
public sealed class CreateIncomeLineModel
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

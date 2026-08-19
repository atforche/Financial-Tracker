namespace Models.Transactions.Update;

/// <summary>
/// Model representing an income line in an update income transaction request.
/// </summary>
public sealed class UpdateIncomeLineModel
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

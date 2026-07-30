namespace Models.Transactions;

/// <summary>
/// Model representing a summary of transactions, including total count and total amount.
/// </summary>
public class TransactionSummaryModel
{
    /// <summary>
    /// Total count of transactions.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Total amount of transactions.
    /// </summary>
    public required decimal TotalAmount { get; init; }
}
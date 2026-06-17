namespace Models.Transactions;

/// <summary>
/// Model representing summary counts and amounts for a specific Transaction Type.
/// </summary>
public class TransactionTrendsTransactionTypeSummaryModel
{
    /// <summary>
    /// Transaction type for this summary.
    /// </summary>
    public required TransactionTypeModel TransactionType { get; init; }

    /// <summary>
    /// Total count of transactions for this type.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Total amount of transactions for this type.
    /// </summary>
    public required decimal TotalAmount { get; init; }
}
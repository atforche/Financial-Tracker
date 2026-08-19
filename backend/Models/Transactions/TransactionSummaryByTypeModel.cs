namespace Models.Transactions;

/// <summary>
/// Model representing a summary of transactions for a specific transaction type.
/// </summary>
public class TransactionSummaryByTypeModel : TransactionSummaryModel
{
    /// <summary>
    /// Transaction type for this summary.
    /// </summary>
    public required TransactionTypeModel TransactionType { get; init; }
}

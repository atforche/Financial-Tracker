namespace Models.Transactions;

/// <summary>
/// Model representing a summary of transactions by date.
/// </summary>
public class TransactionSummaryByDateModel : TransactionSummaryModel
{
    /// <summary>
    /// Date for this summary.
    /// </summary>
    public required DateOnly Date { get; init; }
}
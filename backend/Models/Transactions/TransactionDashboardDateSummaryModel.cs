namespace Models.Transactions;

/// <summary>
/// Model representing summary counts and amounts for a specific date.
/// </summary>
public class TransactionDashboardDateSummaryModel
{
    /// <summary>
    /// Date for this summary.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Total count of transactions for this date.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Total amount of transactions for this date.
    /// </summary>
    public required decimal TotalAmount { get; init; }
}
namespace Models.Transactions;

/// <summary>
/// Model representing the Transaction dashboard response.
/// </summary>
public class TransactionDashboardModel
{
    /// <summary>
    /// Time mode used to build the dashboard response.
    /// </summary>
    public required TransactionDashboardModeModel Mode { get; init; }

    /// <summary>
    /// Matching Transactions for the requested dashboard page.
    /// </summary>
    public required CollectionModel<TransactionModel> Transactions { get; init; }

    /// <summary>
    /// Available Account Names for the current dashboard scope before account-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableAccountNames { get; init; }

    /// <summary>
    /// Available Fund Names for the current dashboard scope before fund-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Summary counts and amounts for each Transaction Type in the requested range.
    /// </summary>
    public required IReadOnlyCollection<TransactionDashboardTransactionTypeSummaryModel> TransactionTypes { get; init; }

    /// <summary>
    /// Summary counts and amounts for each date in the requested range.
    /// </summary>
    public required IReadOnlyCollection<TransactionDashboardDateSummaryModel> Dates { get; init; }
}
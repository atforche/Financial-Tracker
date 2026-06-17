namespace Models.Transactions;

/// <summary>
/// Model representing the Transaction trends response.
/// </summary>
public class TransactionTrendsModel
{
    /// <summary>
    /// Time mode used to build the trends response.
    /// </summary>
    public required TransactionTrendsModeModel Mode { get; init; }

    /// <summary>
    /// Matching Transactions for the requested trends page.
    /// </summary>
    public required CollectionModel<TransactionModel> Transactions { get; init; }

    /// <summary>
    /// Available Account Names for the current trends scope before account-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableAccountNames { get; init; }

    /// <summary>
    /// Available Fund Names for the current trends scope before fund-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Summary counts and amounts for each Transaction Type in the requested range.
    /// </summary>
    public required IReadOnlyCollection<TransactionTrendsTransactionTypeSummaryModel> TransactionTypes { get; init; }

    /// <summary>
    /// Summary counts and amounts for each Accounting Period in the requested range.
    /// </summary>
    public IReadOnlyCollection<TransactionTrendsPeriodSummaryModel>? AccountingPeriods { get; init; }

    /// <summary>
    /// Summary counts and amounts for each date in the requested range.
    /// </summary>
    public IReadOnlyCollection<TransactionTrendsDateSummaryModel>? Dates { get; init; }
}
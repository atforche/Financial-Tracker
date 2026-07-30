namespace Models.Transactions;

/// <summary>
/// Model containing server-side aggregates required for Transaction trend charts and filters.
/// </summary>
public class TransactionTrendsModel
{
    /// <summary>
    /// Available Account Names for the current filters.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableAccountNames { get; init; }

    /// <summary>
    /// Available Fund Names for the current filters.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Summary counts and amounts for each Transaction Type.
    /// </summary>
    public required IReadOnlyCollection<TransactionSummaryByTypeModel> TransactionTypes { get; init; }

    /// <summary>
    /// Summary counts and amounts by Transaction date.
    /// </summary>
    public required IReadOnlyCollection<TransactionSummaryByDateModel> Dates { get; init; }

    /// <summary>
    /// Summary counts and amounts by Accounting Period.
    /// </summary>
    public required IReadOnlyCollection<TransactionSummaryByPeriodModel> AccountingPeriods { get; init; }
}
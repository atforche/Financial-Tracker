namespace Models.Transactions;

/// <summary>
/// Model representing the current Transactions page response.
/// </summary>
public class CurrentTransactionsModel
{
    /// <summary>
    /// Latest Accounting Period identifier, when available.
    /// </summary>
    public required Guid? AccountingPeriodId { get; init; }

    /// <summary>
    /// Latest Accounting Period name, when available.
    /// </summary>
    public required string? AccountingPeriodName { get; init; }

    /// <summary>
    /// Summary counts and amounts for each Transaction Type in the current Accounting Period.
    /// </summary>
    public required IReadOnlyCollection<TransactionTrendsTransactionTypeSummaryModel> TransactionTypes { get; init; }

    /// <summary>
    /// Transactions in the current Accounting Period that are not fully posted.
    /// </summary>
    public required CollectionModel<TransactionModel> UnpostedTransactions { get; init; }

    /// <summary>
    /// Transactions in the current Accounting Period that are fully posted.
    /// </summary>
    public required CollectionModel<TransactionModel> PostedTransactions { get; init; }
}
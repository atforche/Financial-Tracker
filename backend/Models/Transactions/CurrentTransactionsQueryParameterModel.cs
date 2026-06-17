namespace Models.Transactions;

/// <summary>
/// Model representing the query parameters for the current Transactions endpoint.
/// </summary>
public class CurrentTransactionsQueryParameterModel
{
    /// <summary>
    /// Optional Transaction Type filters to apply to the current snapshot.
    /// </summary>
    public IReadOnlyCollection<TransactionTypeModel>? TransactionType { get; init; }

    /// <summary>
    /// Optional Account Name filters to apply to the current snapshot.
    /// </summary>
    public IReadOnlyCollection<string>? AccountName { get; init; }

    /// <summary>
    /// Optional Fund Name filters to apply to the current snapshot.
    /// </summary>
    public IReadOnlyCollection<string>? FundName { get; init; }

    /// <summary>
    /// Optional sort to apply to the not-fully-posted transactions.
    /// </summary>
    public TransactionSortOrderModel? UnpostedTransactionSort { get; init; }

    /// <summary>
    /// Maximum number of not-fully-posted transactions to return.
    /// </summary>
    public int? UnpostedTransactionLimit { get; init; }

    /// <summary>
    /// Number of not-fully-posted transactions to skip.
    /// </summary>
    public int? UnpostedTransactionOffset { get; init; }

    /// <summary>
    /// Optional sort to apply to the fully posted transactions.
    /// </summary>
    public TransactionSortOrderModel? PostedTransactionSort { get; init; }

    /// <summary>
    /// Maximum number of fully posted transactions to return.
    /// </summary>
    public int? PostedTransactionLimit { get; init; }

    /// <summary>
    /// Number of fully posted transactions to skip.
    /// </summary>
    public int? PostedTransactionOffset { get; init; }
}
namespace Models.Transactions;

/// <summary>
/// Model representing the filters that can be applied when retrieving Transactions.
/// </summary>
public class TransactionFilterModel
{
    /// <summary>
    /// Accounting Period IDs to filter the Transactions by
    /// </summary>
    public IReadOnlyCollection<Guid>? AccountingPeriodIds { get; init; }

    /// <summary>
    /// Account IDs to filter the Transactions by
    /// </summary>
    public IReadOnlyCollection<Guid>? AccountIds { get; init; }

    /// <summary>
    /// Fund IDs to filter the Transactions by
    /// </summary>
    public IReadOnlyCollection<Guid>? FundIds { get; init; }

    /// <summary>
    /// Location IDs to filter the Transactions by.
    /// </summary>
    public IReadOnlyCollection<Guid>? LocationIds { get; init; }

    /// <summary>
    /// Transaction Types to filter the Transactions by.
    /// </summary>
    public IReadOnlyCollection<TransactionTypeModel>? Types { get; init; }
}
namespace Domain.Transactions.Queries;

/// <summary>
/// Criteria for querying Transactions over an Accounting Period range.
/// </summary>
public sealed record TransactionAccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId,
    TransactionFilter Filter,
    TransactionSort Sort,
    int Offset,
    int? Limit);

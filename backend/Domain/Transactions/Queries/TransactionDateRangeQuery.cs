namespace Domain.Transactions.Queries;

/// <summary>
/// Criteria for querying Transactions over a date range.
/// </summary>
public sealed record TransactionDateRangeQuery(
    DateOnly Start,
    DateOnly End,
    TransactionFilter Filter,
    TransactionSort Sort,
    int Offset,
    int? Limit);

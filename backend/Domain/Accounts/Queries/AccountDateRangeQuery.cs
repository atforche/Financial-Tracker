namespace Domain.Accounts.Queries;

/// <summary>
/// Criteria for querying Account balances over a date range.
/// </summary>
public sealed record AccountDateRangeQuery(
    DateOnly Start,
    DateOnly End,
    AccountFilter Filter,
    AccountRangeSort Sort,
    int Offset,
    int? Limit);
namespace Domain.Funds.Queries;

/// <summary>
/// Criteria for querying Fund balances over a date range.
/// </summary>
public sealed record FundDateRangeQuery(
    DateOnly Start,
    DateOnly End,
    FundFilter Filter,
    FundRangeSort Sort,
    int Offset,
    int? Limit);
namespace Domain.Funds.Queries;

/// <summary>
/// Criteria for querying Fund balance events over a date range.
/// </summary>
public sealed record FundBalanceEventQuery(
    DateOnly? Start,
    DateOnly? End,
    FundFilter Filter,
    bool IncludePending,
    int Offset,
    int? Limit);

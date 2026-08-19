namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Criteria for querying Accounting Periods.
/// </summary>
public sealed record AccountingPeriodQuery(
    AccountingPeriodFilter Filter,
    AccountingPeriodSort Sort,
    int Offset,
    int? Limit);

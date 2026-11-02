namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Criteria for querying a contiguous range of Accounting Periods.
/// </summary>
public sealed record AccountingPeriodRangeQuery(
    Guid StartId,
    Guid EndId);

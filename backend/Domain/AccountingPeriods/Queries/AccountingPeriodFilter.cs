namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Criteria used to filter Accounting Periods.
/// </summary>
public sealed record AccountingPeriodFilter(
    IReadOnlyCollection<int> Years,
    IReadOnlyCollection<int> Months);

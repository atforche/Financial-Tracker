namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents the result of resolving an Accounting Period range.
/// </summary>
public sealed record AccountingPeriodRangeResolution(
    IReadOnlyCollection<AccountingPeriod>? AccountingPeriods,
    AccountingPeriodRangeQueryFailure Failure);

using Models.AccountingPeriods;

namespace Data.AccountingPeriods;

/// <summary>
/// Failures that can occur while resolving an Accounting Period range.
/// </summary>
[Flags]
public enum AccountingPeriodRangeQueryFailure
{
    /// <summary>
    /// No failure.
    /// </summary>
    None = 0,

    /// <summary>
    /// The start ID was not found.
    /// </summary>
    StartNotFound = 1,

    /// <summary>
    /// The end ID was not found.
    /// </summary>
    EndNotFound = 2,

    /// <summary>
    /// The start occurs after the end.
    /// </summary>
    Reversed = 4,

    /// <summary>
    /// The persisted range contains a gap.
    /// </summary>
    NotContiguous = 8,
}

/// <summary>
/// Result of resolving an Accounting Period range.
/// </summary>
public sealed record AccountingPeriodRangeQueryResult(
    AccountingPeriodsInRangeModel? Model,
    AccountingPeriodRangeQueryFailure Failure);
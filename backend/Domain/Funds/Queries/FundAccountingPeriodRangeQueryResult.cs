using Domain.AccountingPeriods.Queries;

namespace Domain.Funds.Queries;

/// <summary>
/// Result of resolving a Fund Accounting Period range.
/// </summary>
public sealed record FundAccountingPeriodRangeQueryResult(
    FundAccountingPeriodRange? Range,
    AccountingPeriodRangeQueryFailure Failure);
using Domain.AccountingPeriods.Queries;

namespace Domain.Accounts.Queries;

/// <summary>
/// Result of resolving an Account Accounting Period range.
/// </summary>
public sealed record AccountAccountingPeriodRangeQueryResult(
    AccountAccountingPeriodRange? Range,
    AccountingPeriodRangeQueryFailure Failure);

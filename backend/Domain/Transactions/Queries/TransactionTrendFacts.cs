using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;

namespace Domain.Transactions.Queries;

/// <summary>
/// Aggregated Transaction facts used to render trends without loading Transaction details.
/// </summary>
public sealed record TransactionTrendFacts(
    IReadOnlyCollection<string> AvailableAccountNames,
    IReadOnlyCollection<string> AvailableFundNames,
    IReadOnlyCollection<TransactionTypeSummary> TransactionTypes,
    IReadOnlyCollection<TransactionDateSummary> Dates,
    IReadOnlyCollection<TransactionAccountingPeriodSummary> AccountingPeriods);

/// <summary>
/// Count and amount totals for Transactions on a date.
/// </summary>
public sealed record TransactionDateSummary(
    DateOnly Date,
    int TotalCount,
    decimal TotalAmount);

/// <summary>
/// Count and amount totals for Transactions in an Accounting Period.
/// </summary>
public sealed record TransactionAccountingPeriodSummary(
    AccountingPeriodId AccountingPeriodId,
    int TotalCount,
    decimal TotalAmount);

/// <summary>
/// Transaction trend facts paired with the resolved Accounting Periods they reference.
/// </summary>
public sealed record TransactionAccountingPeriodTrendFacts(
    TransactionTrendFacts Trends,
    IReadOnlyCollection<AccountingPeriod> AccountingPeriods);

/// <summary>
/// Result of resolving Transaction trends for an Accounting Period range.
/// </summary>
public sealed record TransactionAccountingPeriodRangeTrendQueryResult(
    TransactionAccountingPeriodTrendFacts? Trends,
    AccountingPeriodRangeQueryFailure Failure);
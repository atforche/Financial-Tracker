using Domain.AccountingPeriods.Queries;

namespace Domain.Transactions.Queries;

/// <summary>
/// Interpreted Transactions and metadata for an Accounting Period range.
/// </summary>
public sealed record TransactionAccountingPeriodRange(
    QueryPage<TransactionDetails> Transactions,
    IReadOnlyCollection<string> AvailableAccountNames,
    IReadOnlyCollection<string> AvailableFundNames,
    IReadOnlyCollection<TransactionTypeSummary> TransactionTypes,
    LocationCashFlow LocationCashFlow,
    int Offset,
    int? Limit);

/// <summary>
/// Result of resolving a Transaction Accounting Period range.
/// </summary>
public sealed record TransactionAccountingPeriodRangeQueryResult(
    TransactionAccountingPeriodRange? Range,
    AccountingPeriodRangeQueryFailure Failure);

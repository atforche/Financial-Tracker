namespace Domain.Transactions.Queries;

/// <summary>
/// Persisted facts for a Transaction date-range result.
/// </summary>
public sealed record TransactionDateRangeFacts(
    TransactionQueryFacts QueryFacts,
    IReadOnlyCollection<string> AvailableAccountNames,
    IReadOnlyCollection<string> AvailableFundNames,
    IReadOnlyCollection<TransactionTypeSummary> TransactionTypes,
    LocationCashFlow LocationCashFlow);

/// <summary>
/// Count and amount totals for a persisted Transaction type.
/// </summary>
public sealed record TransactionTypeSummary(
    TransactionType TransactionType,
    int TotalCount,
    decimal TotalAmount);
namespace Domain.Transactions.Queries;

/// <summary>
/// Persisted facts for a Transaction Accounting Period range.
/// </summary>
public sealed record TransactionAccountingPeriodRangeFacts(
    TransactionQueryFacts QueryFacts,
    IReadOnlyCollection<string> AvailableAccountNames,
    IReadOnlyCollection<string> AvailableFundNames,
    IReadOnlyCollection<TransactionTypeSummary> TransactionTypes);
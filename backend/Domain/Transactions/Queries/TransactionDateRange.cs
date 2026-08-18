namespace Domain.Transactions.Queries;

/// <summary>
/// Interpreted Transactions and supporting date-range metadata.
/// </summary>
public sealed record TransactionDateRange(
    QueryPage<TransactionDetails> Transactions,
    IReadOnlyCollection<string> AvailableAccountNames,
    IReadOnlyCollection<string> AvailableFundNames,
    IReadOnlyCollection<TransactionTypeSummary> TransactionTypes,
    LocationCashFlow LocationCashFlow,
    int Offset,
    int? Limit);

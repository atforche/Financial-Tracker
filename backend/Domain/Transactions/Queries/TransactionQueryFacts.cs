using Domain.AccountingPeriods;

namespace Domain.Transactions.Queries;

/// <summary>
/// A Transaction page and its batched interpretation context.
/// </summary>
public sealed record TransactionQueryFacts(
    QueryPage<Transaction> Transactions,
    IReadOnlyCollection<AccountingPeriod> AccountingPeriods);

using Domain.Accounts;
using Domain.Transactions;

namespace Domain.Overview;

/// <summary>
/// Persisted effect of a Transaction that has not yet posted to one of its Accounts.
/// </summary>
public sealed record OverviewUnpostedEffect(
    AccountId AccountId,
    string AccountName,
    TransactionId TransactionId,
    DateOnly Date,
    decimal PendingDebitAmount,
    decimal PendingCreditAmount);

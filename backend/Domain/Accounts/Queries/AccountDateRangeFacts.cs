namespace Domain.Accounts.Queries;

/// <summary>
/// Persisted Account balance history facts.
/// </summary>
public sealed record AccountDateBalanceFact(AccountId AccountId, DateOnly Date, int Sequence, decimal PostedBalance);

/// <summary>
/// Persisted income destination facts for a date range.
/// </summary>
public sealed record AccountDateRangeIncomeFact(
    decimal Amount,
    AccountType AccountType,
    bool HasInternalSource,
    DateOnly? PostedDate);

/// <summary>
/// Persisted spending facts for a date range.
/// </summary>
public sealed record AccountDateRangeSpendingFact(decimal Amount, DateOnly? PostedDate);
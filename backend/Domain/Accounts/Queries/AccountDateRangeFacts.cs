namespace Domain.Accounts.Queries;

/// <summary>
/// Persisted Account balance history facts.
/// </summary>
public sealed record AccountDateBalanceFact(AccountId AccountId, DateOnly Date, int Sequence, decimal PostedBalance);
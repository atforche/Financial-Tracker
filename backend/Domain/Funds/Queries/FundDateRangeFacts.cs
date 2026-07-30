namespace Domain.Funds.Queries;

/// <summary>
/// Persisted Fund balance history facts.
/// </summary>
public sealed record FundDateBalanceFact(FundId FundId, DateOnly Date, int Sequence, decimal PostedBalance);
using Domain.Accounts;

namespace Domain.Funds.Queries;

/// <summary>
/// Persisted Fund balance history facts.
/// </summary>
public sealed record FundDateBalanceFact(FundId FundId, DateOnly Date, int Sequence, decimal PostedBalance);

/// <summary>
/// Persisted income destination facts for a date range.
/// </summary>
public sealed record FundDateRangeIncomeFact(
    decimal Amount,
    AccountType AccountType,
    bool HasInternalSource,
    DateOnly? PostedDate);

/// <summary>
/// Persisted spending facts for a date range.
/// </summary>
public sealed record FundDateRangeSpendingFact(decimal Amount, DateOnly? PostedDate);
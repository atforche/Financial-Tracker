using Domain.AccountingPeriods;

namespace Domain.Funds.Queries;

/// <summary>
/// Persisted Fund balance facts for an Accounting Period.
/// </summary>
public sealed record FundPeriodBalanceFacts(
    AccountingPeriod AccountingPeriod,
    IReadOnlyCollection<FundPeriodBalanceFact> Balances);

/// <summary>
/// Persisted Fund balance facts within an Accounting Period.
/// </summary>
public sealed record FundPeriodBalanceFact(Fund Fund, decimal OpeningBalance, decimal ClosingBalance);
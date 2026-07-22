using Domain.AccountingPeriods;

namespace Domain.Accounts.Queries;

/// <summary>
/// Persisted Account balance facts for an Accounting Period.
/// </summary>
public sealed record AccountPeriodBalanceFacts(
    AccountingPeriod AccountingPeriod,
    IReadOnlyCollection<AccountPeriodBalanceFact> Balances);

/// <summary>
/// Persisted Account balance facts within an Accounting Period.
/// </summary>
public sealed record AccountPeriodBalanceFact(Account Account, decimal OpeningBalance, decimal ClosingBalance);
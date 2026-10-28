using Domain.Accounts.Queries;
using Domain.Funds.Queries;

namespace Domain.Overview;

/// <summary>
/// Represents Account and Fund balance summaries over a date range.
/// </summary>
public sealed record OverviewDateRange(
    DateOnly Start,
    AccountBalanceSummary AccountOpeningBalance,
    AccountBalanceSummary AccountClosingBalance,
    IReadOnlyCollection<OverviewDateBalance> AccountBalances,
    FundBalanceSummary FundClosingBalance,
    IReadOnlyCollection<OverviewDateBalance> FundBalances,
    BalanceGranularity Granularity);

/// <summary>
/// Represents a total balance at the end of a date.
/// </summary>
public sealed record OverviewDateBalance(DateOnly Date, decimal TotalBalance);

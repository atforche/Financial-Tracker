using Domain.AccountingPeriods;
using Domain.Accounts.Queries;
using Domain.Funds.Queries;

namespace Domain.Overview;

/// <summary>
/// Represents Account and Fund balance summaries over a date range.
/// </summary>
public sealed record OverviewDateRange(
    AccountBalanceSummary AccountOpeningBalance,
    AccountBalanceSummary AccountClosingBalance,
    IReadOnlyCollection<OverviewDateBalance> AccountBalances,
    FundBalanceSummary FundOpeningBalance,
    FundBalanceSummary FundClosingBalance,
    IReadOnlyCollection<OverviewDateBalance> FundBalances,
    BalanceGranularity Granularity);

/// <summary>
/// Represents a total balance at the end of a date.
/// </summary>
public sealed record OverviewDateBalance(DateOnly Date, decimal TotalBalance);

/// <summary>
/// Represents Account and Fund balance summaries over an Accounting Period range.
/// </summary>
public sealed record OverviewAccountingPeriodRange(
    AccountBalanceSummary AccountOpeningBalance,
    AccountBalanceSummary AccountClosingBalance,
    IReadOnlyCollection<OverviewPeriodBalance> AccountBalances,
    FundBalanceSummary FundOpeningBalance,
    FundBalanceSummary FundClosingBalance,
    IReadOnlyCollection<OverviewPeriodBalance> FundBalances);

/// <summary>
/// Represents total opening and closing balances for an Accounting Period.
/// </summary>
public sealed record OverviewPeriodBalance(AccountingPeriod AccountingPeriod, decimal OpeningBalance, decimal ClosingBalance);

using Domain.AccountingPeriods;

namespace Domain.Funds.Queries;

/// <summary>
/// Represents Fund balances and totals over an Accounting Period range.
/// </summary>
public sealed record FundAccountingPeriodRange(
    QueryPage<FundRangeBalance> Funds,
    IReadOnlyCollection<string> AvailableFundNames,
    decimal TotalIncome,
    decimal TrackedIncome,
    decimal TotalSpending,
    IReadOnlyCollection<FundPeriodBalanceSummary> AccountingPeriods)
{
    /// <summary>
    /// Gets income deposited into untracked Accounts.
    /// </summary>
    public decimal UntrackedIncome => TotalIncome - TrackedIncome;
}

/// <summary>
/// Represents a Fund's balances at the boundaries of a range.
/// </summary>
public sealed record FundRangeBalance(Fund Fund, decimal StartingBalance, decimal EndingBalance);

/// <summary>
/// Represents Fund balance summaries for an Accounting Period.
/// </summary>
public sealed record FundPeriodBalanceSummary(
    AccountingPeriod AccountingPeriod,
    FundBalanceSummary OpeningBalance,
    FundBalanceSummary ClosingBalance);

/// <summary>
/// Represents an interpreted summary of Fund balances.
/// </summary>
public sealed record FundBalanceSummary(
    decimal TotalBalance,
    decimal TotalAssignedBalance,
    decimal TotalUnassignedBalance);
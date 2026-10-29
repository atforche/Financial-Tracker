using Domain.Accounts.Queries;
using Domain.Funds.Queries;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents everything shown for a single Accounting Period: its balance and totals, how many of its Fund Goals have
/// their expected contribution reduced by a maximum ending balance, its neighbors, and how its Account and Fund
/// balances changed over it.
/// </summary>
public sealed record AccountingPeriodDetail(
    AccountingPeriodBalance Balance,
    int CappedGoalCount,
    AccountingPeriod? PreviousPeriod,
    AccountingPeriod? NextPeriod,
    decimal TotalSpending,
    AccountBalanceSummary AccountOpeningBalance,
    AccountBalanceSummary AccountClosingBalance,
    FundBalanceSummary FundOpeningBalance,
    FundBalanceSummary FundClosingBalance);

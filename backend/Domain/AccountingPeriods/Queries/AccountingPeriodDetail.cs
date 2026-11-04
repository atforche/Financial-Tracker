using Domain.AccountGoals;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.FundGoals;
using Domain.Funds;
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
    AccountingPeriodGoalSummary GoalSummary,
    AccountingPeriod? PreviousPeriod,
    AccountingPeriod? NextPeriod,
    decimal TotalSpending,
    AccountBalanceSummary AccountOpeningBalance,
    AccountBalanceSummary AccountClosingBalance,
    FundBalanceSummary FundOpeningBalance,
    FundBalanceSummary FundClosingBalance,
    IReadOnlyCollection<AccountingPeriodAccount> Accounts,
    IReadOnlyCollection<AccountingPeriodFund> Funds);

/// <summary>
/// Represents an Account's balances and goal progress in an Accounting Period.
/// </summary>
public sealed record AccountingPeriodAccount(
    Account Account,
    decimal OpeningBalance,
    decimal ClosingBalance,
    AccountGoalProgress? GoalProgress);

/// <summary>
/// Represents a Fund's balances and goal progress in an Accounting Period.
/// </summary>
public sealed record AccountingPeriodFund(
    Fund Fund,
    decimal OpeningBalance,
    decimal ClosingBalance,
    FundGoalProgress? GoalProgress);

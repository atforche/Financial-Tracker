using Domain.AccountingPeriods;

namespace Domain.Overview;

/// <summary>
/// Everything shown on the overview as of a date.
/// </summary>
public sealed record OverviewSummary(
    OverviewDateRange History,
    OverviewPeriod? CurrentPeriod,
    IReadOnlyCollection<OverviewAttentionGroup> Attention);

/// <summary>
/// Income, goal contribution, and spending figures for the most recent open Accounting Period.
/// </summary>
public sealed record OverviewPeriod(
    AccountingPeriod AccountingPeriod,
    decimal ExpectedIncome,
    decimal ActualIncome,
    decimal ActualTrackedIncome,
    decimal ExpectedGoalContributions,
    decimal ActualGoalContributions,
    decimal TotalSpending);

/// <summary>
/// Kinds of items the overview flags as needing attention.
/// </summary>
public enum OverviewAttentionKind
{
    /// <summary>
    /// Fund or Account Goals whose balance is below their minimum.
    /// </summary>
    GoalsBelowMinimum,

    /// <summary>
    /// Fund or Account Goals whose balance is above their maximum.
    /// </summary>
    GoalsAboveMaximum,

    /// <summary>
    /// Fund Goals still short of their expected contribution.
    /// </summary>
    GoalsBehind,

    /// <summary>
    /// Transactions that have not posted to every Account they affect.
    /// </summary>
    UnpostedTransactions,

    /// <summary>
    /// Expected income whose dates have passed without that much income being received.
    /// </summary>
    IncomeNotReceived,
}

/// <summary>
/// A group of related items needing attention, with its largest items. Count is the number of goals, Transactions,
/// or passed expected income payments; ItemCount is the number of items the top items were chosen from, which differs
/// for unposted Transactions, whose items are their Accounts. Income is set only for income not received.
/// </summary>
public sealed record OverviewAttentionGroup(
    OverviewAttentionKind Kind,
    int Count,
    decimal Amount,
    int ItemCount,
    IReadOnlyCollection<OverviewAttentionItem> TopItems,
    DateOnly? OldestDate,
    OverviewIncomeShortfall? Income = null);

/// <summary>
/// Tracked income expected so far in the current Accounting Period, and how much of it has been received.
/// </summary>
public sealed record OverviewIncomeShortfall(decimal ExpectedAmount, decimal ReceivedAmount);

/// <summary>
/// A single item needing attention. Count is set when the item stands for several Transactions.
/// </summary>
public sealed record OverviewAttentionItem(string Name, decimal Amount, int? Count);

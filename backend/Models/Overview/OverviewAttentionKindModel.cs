namespace Models.Overview;

/// <summary>
/// Enum representing the kinds of items the overview flags as needing attention
/// </summary>
public enum OverviewAttentionKindModel
{
    /// <summary>
    /// Fund or Account Goals whose balance is below their minimum
    /// </summary>
    GoalsBelowMinimum,

    /// <summary>
    /// Fund or Account Goals whose balance is above their maximum
    /// </summary>
    GoalsAboveMaximum,

    /// <summary>
    /// Fund Goals still short of their expected contribution
    /// </summary>
    GoalsBehind,

    /// <summary>
    /// Transactions that have not posted to every Account they affect
    /// </summary>
    UnpostedTransactions,

    /// <summary>
    /// Expected income whose dates have passed without that much income being received
    /// </summary>
    IncomeNotReceived,
}

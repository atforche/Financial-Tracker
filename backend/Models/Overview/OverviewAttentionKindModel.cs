namespace Models.Overview;

/// <summary>
/// Enum representing the kinds of items the overview flags as needing attention
/// </summary>
public enum OverviewAttentionKindModel
{
    /// <summary>
    /// Fund Goals whose balance is outside their range.
    /// </summary>
    FundGoalsOutOfRange,

    /// <summary>
    /// Account Goals whose balance is outside their range.
    /// </summary>
    AccountGoalsOutOfRange,

    /// <summary>
    /// Fund Goals still short of their expected contribution
    /// </summary>
    GoalsBehind,

    /// <summary>
    /// Transactions that have not posted to every Account they affect
    /// </summary>
    UnpostedTransactions,

    /// <summary>
    /// Expected Income Sources whose dates have passed without that much income being received from them
    /// </summary>
    IncomeNotReceived,
}

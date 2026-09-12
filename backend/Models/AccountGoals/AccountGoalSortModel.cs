namespace Models.AccountGoals;

/// <summary>
/// Available ordering options for Account Goals.
/// </summary>
public enum AccountGoalSortModel
{
    /// <summary>
    /// Sorts by Account name.
    /// </summary>
    Account,

    /// <summary>
    /// Sorts by Account name descending.
    /// </summary>
    AccountDescending,

    /// <summary>
    /// Sorts by minimum ending balance.
    /// </summary>
    MinimumEndingBalance,

    /// <summary>
    /// Sorts by minimum ending balance descending.
    /// </summary>
    MinimumEndingBalanceDescending,

    /// <summary>
    /// Sorts by maximum ending balance.
    /// </summary>
    MaximumEndingBalance,

    /// <summary>
    /// Sorts by maximum ending balance descending.
    /// </summary>
    MaximumEndingBalanceDescending,
}

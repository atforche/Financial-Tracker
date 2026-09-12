namespace Domain.AccountGoals.Queries;

/// <summary>
/// Supported Account Goal sort orders.
/// </summary>
public enum AccountGoalSort
{
    /// <summary>
    /// Sorts Account Goals by Account name in ascending order.
    /// </summary>
    Account,

    /// <summary>
    /// Sorts Account Goals by Account name in descending order.
    /// </summary>
    AccountDescending,

    /// <summary>
    /// Sorts Account Goals by minimum ending balance in ascending order.
    /// </summary>
    MinimumEndingBalance,

    /// <summary>
    /// Sorts Account Goals by minimum ending balance in descending order.
    /// </summary>
    MinimumEndingBalanceDescending,

    /// <summary>
    /// Sorts Account Goals by maximum ending balance in ascending order.
    /// </summary>
    MaximumEndingBalance,

    /// <summary>
    /// Sorts Account Goals by maximum ending balance in descending order.
    /// </summary>
    MaximumEndingBalanceDescending,
}

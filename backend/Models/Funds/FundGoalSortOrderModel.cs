namespace Models.Funds;

/// <summary>
/// Enum representing the different ways Fund Goals can be sorted
/// </summary>
public enum FundGoalSortOrderModel
{
    /// <summary>
    /// Fund Goals are sorted by accounting period in ascending order
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Fund Goals are sorted by accounting period in descending order
    /// </summary>
    AccountingPeriodDescending,

    /// <summary>
    /// Fund Goals are sorted by goal amount in ascending order
    /// </summary>
    GoalAmount,

    /// <summary>
    /// Fund Goals are sorted by goal amount in descending order
    /// </summary>
    GoalAmountDescending,
}
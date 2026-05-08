namespace Models.Goals;

/// <summary>
/// Enum representing the different ways Goals can be sorted
/// </summary>
public enum GoalSortOrderModel
{
    /// <summary>
    /// Goals are sorted by accounting period in ascending order
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Goals are sorted by accounting period in descending order
    /// </summary>
    AccountingPeriodDescending,

    /// <summary>
    /// Goals are sorted by goal amount in ascending order
    /// </summary>
    GoalAmount,

    /// <summary>
    /// Goals are sorted by goal amount in descending order
    /// </summary>
    GoalAmountDescending,
}
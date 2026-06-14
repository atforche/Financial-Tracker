namespace Models.Goals;

/// <summary>
/// Enum representing the different ways Assignment Goals can be sorted
/// </summary>
public enum AssignmentGoalSortOrderModel
{
    /// <summary>
    /// Assignment Goals are sorted by accounting period in ascending order
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Assignment Goals are sorted by accounting period in descending order
    /// </summary>
    AccountingPeriodDescending,

    /// <summary>
    /// Assignment Goals are sorted by fund in ascending order
    /// </summary>
    Fund,

    /// <summary>
    /// Assignment Goals are sorted by fund in descending order
    /// </summary>
    FundDescending,

    /// <summary>
    /// Assignment Goals are sorted by type in ascending order
    /// </summary>
    Type,

    /// <summary>
    /// Assignment Goals are sorted by type in descending order
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Assignment Goals are sorted by goal amount in ascending order
    /// </summary>
    GoalAmount,

    /// <summary>
    /// Assignment Goals are sorted by goal amount in descending order
    /// </summary>
    GoalAmountDescending,
}
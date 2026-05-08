namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the different ways Goals within an Accounting Period can be sorted
/// </summary>
public enum AccountingPeriodGoalSortOrderModel
{
    /// <summary>
    /// Goals are sorted by name in ascending order
    /// </summary>
    Name,

    /// <summary>
    /// Goals are sorted by name in descending order
    /// </summary>
    NameDescending,

    /// <summary>
    /// Goals are sorted by goal type in ascending order
    /// </summary>
    Type,

    /// <summary>
    /// Goals are sorted by goal type in descending order
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Goals are sorted by goal amount in ascending order
    /// </summary>
    GoalAmount,

    /// <summary>
    /// Goals are sorted by goal amount in descending order
    /// </summary>
    GoalAmountDescending,

    /// <summary>
    /// Goals are sorted by remaining amount to assign in ascending order
    /// </summary>
    RemainingAmountToAssign,

    /// <summary>
    /// Goals are sorted by remaining amount to assign in descending order
    /// </summary>
    RemainingAmountToAssignDescending,

    /// <summary>
    /// Goals are sorted by whether the assignment goal is met in ascending order
    /// </summary>
    IsAssignmentGoalMet,

    /// <summary>
    /// Goals are sorted by whether the assignment goal is met in descending order
    /// </summary>
    IsAssignmentGoalMetDescending,

    /// <summary>
    /// Goals are sorted by remaining amount to spend in ascending order
    /// </summary>
    RemainingAmountToSpend,

    /// <summary>
    /// Goals are sorted by remaining amount to spend in descending order
    /// </summary>
    RemainingAmountToSpendDescending,

    /// <summary>
    /// Goals are sorted by whether the spending goal is met in ascending order
    /// </summary>
    IsSpendingGoalMet,

    /// <summary>
    /// Goals are sorted by whether the spending goal is met in descending order
    /// </summary>
    IsSpendingGoalMetDescending,
}
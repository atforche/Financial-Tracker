namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the different ways Fund Goals within an Accounting Period can be sorted
/// </summary>
public enum AccountingPeriodFundGoalSortOrderModel
{
    /// <summary>
    /// Fund Goals are sorted by name in ascending order
    /// </summary>
    Name,

    /// <summary>
    /// Fund Goals are sorted by name in descending order
    /// </summary>
    NameDescending,

    /// <summary>
    /// Fund Goals are sorted by goal type in ascending order
    /// </summary>
    Type,

    /// <summary>
    /// Fund Goals are sorted by goal type in descending order
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Fund Goals are sorted by goal amount in ascending order
    /// </summary>
    GoalAmount,

    /// <summary>
    /// Fund Goals are sorted by goal amount in descending order
    /// </summary>
    GoalAmountDescending,

    /// <summary>
    /// Fund Goals are sorted by remaining amount to assign in ascending order
    /// </summary>
    RemainingAmountToAssign,

    /// <summary>
    /// Fund Goals are sorted by remaining amount to assign in descending order
    /// </summary>
    RemainingAmountToAssignDescending,

    /// <summary>
    /// Fund Goals are sorted by whether the assignment goal is met in ascending order
    /// </summary>
    IsAssignmentGoalMet,

    /// <summary>
    /// Fund Goals are sorted by whether the assignment goal is met in descending order
    /// </summary>
    IsAssignmentGoalMetDescending,

    /// <summary>
    /// Fund Goals are sorted by remaining amount to spend in ascending order
    /// </summary>
    RemainingAmountToSpend,

    /// <summary>
    /// Fund Goals are sorted by remaining amount to spend in descending order
    /// </summary>
    RemainingAmountToSpendDescending,

    /// <summary>
    /// Fund Goals are sorted by whether the spending goal is met in ascending order
    /// </summary>
    IsSpendingGoalMet,

    /// <summary>
    /// Fund Goals are sorted by whether the spending goal is met in descending order
    /// </summary>
    IsSpendingGoalMetDescending,
}
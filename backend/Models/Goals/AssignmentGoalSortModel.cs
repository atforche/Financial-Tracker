namespace Models.Goals;

/// <summary>
/// Enum representing the different ways Assignment Goals can be sorted
/// </summary>
public enum AssignmentGoalSortModel
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

    /// <summary>
    /// Assignment Goals are sorted by total amount to assign in ascending order
    /// </summary>
    TotalAmountToAssign,

    /// <summary>
    /// Assignment Goals are sorted by total amount to assign in descending order
    /// </summary>
    TotalAmountToAssignDescending,

    /// <summary>
    /// Assignment Goals are sorted by total amount assigned in ascending order
    /// </summary>
    TotalAmountAssigned,

    /// <summary>
    /// Assignment Goals are sorted by total amount assigned in descending order
    /// </summary>
    TotalAmountAssignedDescending,

    /// <summary>
    /// Assignment Goals are sorted by whether or not they are met, with unmet goals appearing first
    /// </summary>
    IsMet,

    /// <summary>
    /// Assignment Goals are sorted by whether or not they are met, with met goals appearing first
    /// </summary>
    IsMetDescending,
}
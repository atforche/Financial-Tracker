namespace Models.Goals;

/// <summary>
/// Enum representing the different ways Spending Goals can be sorted
/// </summary>
public enum SpendingGoalSortModel
{
    /// <summary>
    /// Spending Goals are sorted by accounting period in ascending order
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Spending Goals are sorted by accounting period in descending order
    /// </summary>
    AccountingPeriodDescending,

    /// <summary>
    /// Spending Goals are sorted by fund in ascending order
    /// </summary>
    Fund,

    /// <summary>
    /// Spending Goals are sorted by fund in descending order
    /// </summary>
    FundDescending,

    /// <summary>
    /// Spending Goals are sorted by type in ascending order
    /// </summary>
    Type,

    /// <summary>
    /// Spending Goals are sorted by type in descending order
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Spending Goals are sorted by total amount to spend in ascending order
    /// </summary>
    TotalAmountToSpend,

    /// <summary>
    /// Spending Goals are sorted by total amount to spend in descending order
    /// </summary>
    TotalAmountToSpendDescending,

    /// <summary>
    /// Spending Goals are sorted by total amount spent in ascending order
    /// </summary>
    TotalAmountSpent,

    /// <summary>
    /// Spending Goals are sorted by total amount spent in descending order
    /// </summary>
    TotalAmountSpentDescending,

    /// <summary>
    /// Spending Goals are sorted by whether or not they are met, with unmet goals appearing first
    /// </summary>
    IsMet,

    /// <summary>
    /// Spending Goals are sorted by whether or not they are met, with met goals appearing first
    /// </summary>
    IsMetDescending,
}
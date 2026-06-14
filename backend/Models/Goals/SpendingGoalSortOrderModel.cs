namespace Models.Goals;

/// <summary>
/// Enum representing the different ways Spending Goals can be sorted
/// </summary>
public enum SpendingGoalSortOrderModel
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
}
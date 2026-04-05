namespace Domain.Budgets;

/// <summary>
/// Enum representing the type of a Budget
/// </summary>
public enum BudgetType
{
    /// <summary>
    /// A budget that resets each month
    /// </summary>
    Monthly,

    /// <summary>
    /// A budget that rolls over unspent amounts to the next period
    /// </summary>
    Rolling,

    /// <summary>
    /// A budget that tracks savings toward a goal
    /// </summary>
    Savings,

    /// <summary>
    /// A budget that tracks debt repayment
    /// </summary>
    Debt,
}

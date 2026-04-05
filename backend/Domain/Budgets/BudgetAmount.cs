namespace Domain.Budgets;

/// <summary>
/// Value object class representing a Budget Amount.
/// A Budget Amount represents a monetary amount associated with a particular Budget.
/// </summary>
public class BudgetAmount
{
    /// <summary>
    /// Budget for this Budget Amount
    /// </summary>
    public required Budget Budget { get; init; }

    /// <summary>
    /// Amount for this Budget Amount
    /// </summary>
    public required decimal Amount { get; init; }
}

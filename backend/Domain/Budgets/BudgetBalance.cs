namespace Domain.Budgets;

/// <summary>
/// Value object class representing the balance of a Budget Goal
/// </summary>
public class BudgetBalance
{
    /// <summary>
    /// Budget Goal for this Budget Balance
    /// </summary>
    public BudgetGoal BudgetGoal { get; }

    /// <summary>
    /// Posted Balance for this Budget Balance
    /// </summary>
    public decimal PostedBalance { get; }

    /// <summary>
    /// Available to spend for this Budget Balance, or null if not applicable for this budget type
    /// </summary>
    public decimal? AvailableToSpend { get; }

    /// <summary>
    /// Adds the provided pending amount to this Budget Balance
    /// </summary>
    internal BudgetBalance AddNewPendingAmount(decimal amount) =>
        new(BudgetGoal, PostedBalance, AvailableToSpend.HasValue ? AvailableToSpend + amount : null);

    /// <summary>
    /// Posts the provided pending amount to this Budget Balance
    /// </summary>
    internal BudgetBalance PostPendingAmount(decimal amount) =>
        new(BudgetGoal, PostedBalance + amount, AvailableToSpend.HasValue ? AvailableToSpend + amount : null);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal BudgetBalance(BudgetGoal budgetGoal, decimal postedBalance, decimal? availableToSpend)
    {
        BudgetGoal = budgetGoal;
        PostedBalance = postedBalance;
        AvailableToSpend = budgetGoal.Budget.Type is BudgetType.Savings or BudgetType.Debt ? null : availableToSpend;
    }
}

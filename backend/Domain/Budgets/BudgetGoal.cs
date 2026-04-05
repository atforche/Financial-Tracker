using Domain.AccountingPeriods;

namespace Domain.Budgets;

/// <summary>
/// Entity class representing the state of a Budget within a particular accounting period.
/// </summary>
public class BudgetGoal : Entity<BudgetGoalId>
{
    private List<BudgetBalanceHistory> _budgetBalanceHistories = [];

    /// <summary>
    /// Budget ID that this goal belongs to
    /// </summary>
    public BudgetId BudgetId { get; private set; }

    /// <summary>
    /// Accounting Period ID that this goal belongs to
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <summary>
    /// Target amount for this goal
    /// </summary>
    public decimal GoalAmount { get; internal set; }

    /// <summary>
    /// Whether the goal amount has been met
    /// </summary>
    public bool IsGoalMet { get; internal set; }

    /// <summary>
    /// Collection of balance history entries for this goal
    /// </summary>
    public IReadOnlyCollection<BudgetBalanceHistory> BudgetBalanceHistories
    {
        get => _budgetBalanceHistories;
        private set => _budgetBalanceHistories = value.ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal BudgetGoal(BudgetId budgetId, AccountingPeriodId accountingPeriodId, decimal goalAmount)
        : base(new BudgetGoalId(Guid.NewGuid()))
    {
        BudgetId = budgetId;
        AccountingPeriodId = accountingPeriodId;
        GoalAmount = goalAmount;
        IsGoalMet = false;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private BudgetGoal()
        : base()
    {
        BudgetId = null!;
        AccountingPeriodId = null!;
        _budgetBalanceHistories = null!;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="BudgetGoal"/>
/// </summary>
public record BudgetGoalId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal BudgetGoalId(Guid value) : base(value) { }
}

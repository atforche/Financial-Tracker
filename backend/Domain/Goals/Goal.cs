using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Entity class representing the goal for a Fund within a particular Accounting Period.
/// </summary>
public class Goal : Entity<GoalId>
{
    /// <summary>
    /// Fund for this Goal
    /// </summary>
    public Fund Fund { get; private set; }

    /// <summary>
    /// Accounting Period ID for this Goal
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <summary>
    /// Type for this Goal
    /// </summary>
    public GoalType GoalType { get; private set; }

    /// <summary>
    /// Target amount for this Goal
    /// </summary>
    public decimal GoalAmount { get; private set; }

    /// <summary>
    /// Remaining amount to assign for this Goal
    /// </summary>
    public decimal RemainingAmountToAssign { get; private set; }

    /// <summary>
    /// Remaining amount to assign for this Goal including pending assigned amounts
    /// </summary>
    public decimal RemainingAmountToAssignIncludingPending { get; private set; }

    /// <summary>
    /// Indicates whether the assignment goal has been met for this Goal
    /// </summary>
    public bool IsAssignmentGoalMet { get; private set; }

    /// <summary>
    /// Indicates whether the assignment goal has been met for this Goal including pending assigned amounts
    /// </summary>
    public bool IsAssignmentGoalMetIncludingPending { get; private set; }

    /// <summary>
    /// Remaining amount to spend for this Goal
    /// </summary>
    public decimal RemainingAmountToSpend { get; private set; }

    /// <summary>
    /// Remaining amount to spend for this Goal including pending spent amounts
    /// </summary>
    public decimal RemainingAmountToSpendIncludingPending { get; private set; }

    /// <summary>
    /// Indicates whether the spending goal has been met for this Goal
    /// </summary>
    public bool IsSpendingGoalMet { get; private set; }

    /// <summary>
    /// Indicates whether the spending goal has been met for this Goal including pending spent amounts
    /// </summary>
    public bool IsSpendingGoalMetIncludingPending { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Goal(
        Fund fund,
        AccountingPeriodId accountingPeriodId,
        GoalType goalType,
        decimal goalAmount)
        : base(new GoalId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriodId = accountingPeriodId;
        GoalType = goalType;
        GoalAmount = goalAmount;
    }

    /// <summary>
    /// Updates this Goal
    /// </summary>
    internal void UpdateGoal(GoalType goalType, decimal goalAmount, AccountingPeriodFundBalanceHistory balanceHistory)
    {
        GoalType = goalType;
        GoalAmount = goalAmount;
        EvaluateGoal(balanceHistory);
    }

    /// <summary>
    /// Evaluates progress towards the goal
    /// </summary>
    internal void EvaluateGoal(AccountingPeriodFundBalanceHistory balanceHistory)
    {
        if (GoalType == GoalType.Monthly)
        {
            RemainingAmountToAssign = GoalAmount - balanceHistory.OpeningBalance - balanceHistory.AmountAssigned;
            RemainingAmountToSpend = -1 * balanceHistory.ClosingBalance;
        }
        else if (GoalType == GoalType.Rolling)
        {
            RemainingAmountToAssign = GoalAmount - balanceHistory.AmountAssigned;
            RemainingAmountToSpend = -1 * balanceHistory.ClosingBalance;
        }
        else if (GoalType == GoalType.Savings)
        {
            RemainingAmountToAssign = GoalAmount - balanceHistory.AmountAssigned;
            RemainingAmountToSpend = 0;
        }
        else if (GoalType == GoalType.Debt)
        {
            RemainingAmountToAssign = GoalAmount - balanceHistory.AmountAssigned;
            RemainingAmountToSpend = balanceHistory.ClosingBalance;
        }
        RemainingAmountToAssignIncludingPending = RemainingAmountToAssign - balanceHistory.PendingAmountAssigned;
        RemainingAmountToSpendIncludingPending = RemainingAmountToSpend + balanceHistory.PendingAmountSpent;

        IsAssignmentGoalMet = RemainingAmountToAssign <= 0;
        IsAssignmentGoalMetIncludingPending = RemainingAmountToAssignIncludingPending <= 0;
        IsSpendingGoalMet = RemainingAmountToSpend >= 0;
        IsSpendingGoalMetIncludingPending = RemainingAmountToSpendIncludingPending >= 0;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Goal()
        : base()
    {
        Fund = null!;
        AccountingPeriodId = null!;
        GoalType = default;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="Goal"/>
/// </summary>
public record GoalId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal GoalId(Guid value) : base(value) { }
}
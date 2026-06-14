using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Entity class representing the assignment goal for a Fund within a particular Accounting Period.
/// </summary>
public class AssignmentGoal : Entity<AssignmentGoalId>
{
    /// <summary>
    /// Fund for this Assignment Goal
    /// </summary>
    public Fund Fund { get; private set; }

    /// <summary>
    /// Accounting Period ID for this Assignment Goal
    /// </summary>
    public AccountingPeriodId? AccountingPeriodId { get; private set; }

    /// <summary>
    /// Type for this Assignment Goal
    /// </summary>
    public AssignmentGoalType AssignmentGoalType { get; private set; }

    /// <summary>
    /// Goal amount for this Assignment Goal
    /// </summary>
    public decimal GoalAmount { get; private set; }

    /// <summary>
    /// Total amount to assign for this Assignment Goal
    /// </summary>
    public decimal TotalAmountToAssign { get; private set; }

    /// <summary>
    /// Total amount assigned for this Assignment Goal
    /// </summary>
    public decimal TotalAmountAssigned { get; private set; }

    /// <summary>
    /// Total amount assigned for this Assignment Goal including pending assigned amounts
    /// </summary>
    public decimal TotalAmountAssignedIncludingPending { get; private set; }

    /// <summary>
    /// Indicates whether the assignment goal has been met
    /// </summary>
    public bool IsGoalMet { get; private set; }

    /// <summary>
    /// Indicates whether the assignment goal has been met including pending assigned amounts
    /// </summary>
    public bool IsGoalMetIncludingPending { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AssignmentGoal(
        Fund fund,
        AccountingPeriodId? accountingPeriodId,
        AssignmentGoalType assignmentGoalType,
        decimal goalAmount)
        : base(new AssignmentGoalId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriodId = accountingPeriodId;
        AssignmentGoalType = assignmentGoalType;
        GoalAmount = goalAmount;
    }

    /// <summary>
    /// Updates this Assignment Goal
    /// </summary>
    internal void UpdateGoal(AssignmentGoalType assignmentGoalType, decimal goalAmount, AccountingPeriodFundBalanceHistory balanceHistory)
    {
        AssignmentGoalType = assignmentGoalType;
        GoalAmount = goalAmount;
        EvaluateGoal(balanceHistory);
    }

    /// <summary>
    /// Evaluates progress towards the assignment goal
    /// </summary>
    internal void EvaluateGoal(AccountingPeriodFundBalanceHistory balanceHistory)
    {
        TotalAmountToAssign = AssignmentGoalType switch
        {
            AssignmentGoalType.MonthlyTarget => GoalAmount - balanceHistory.OpeningBalance,
            AssignmentGoalType.RecurringContribution => GoalAmount,
            _ => throw new InvalidOperationException($"Unsupported assignment goal type '{AssignmentGoalType}'."),
        };
        TotalAmountAssigned = balanceHistory.AmountAssigned;
        TotalAmountAssignedIncludingPending = TotalAmountAssigned + balanceHistory.PendingAmountAssigned;
        IsGoalMet = TotalAmountAssigned >= TotalAmountToAssign;
        IsGoalMetIncludingPending = TotalAmountAssignedIncludingPending >= TotalAmountToAssign;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AssignmentGoal()
        : base()
    {
        Fund = null!;
        AccountingPeriodId = null;
        AssignmentGoalType = default;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AssignmentGoal"/>
/// </summary>
public record AssignmentGoalId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AssignmentGoalId(Guid value) : base(value) { }
}
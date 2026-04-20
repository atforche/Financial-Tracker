using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Entity class representing the goal for a Fund within a particular Accounting Period.
/// </summary>
public class FundGoal : Entity<FundGoalId>
{
    /// <summary>
    /// Fund for this Fund Goal
    /// </summary>
    public Fund Fund { get; private set; }

    /// <summary>
    /// Accounting Period ID for this Fund Goal
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <summary>
    /// Type for this Fund Goal
    /// </summary>
    public FundGoalType GoalType
    {
        get;
        internal set
        {
            field = value;
            EvaluateGoal();
        }
    }

    /// <summary>
    /// Target amount for this Fund Goal
    /// </summary>
    public decimal GoalAmount
    {
        get;
        internal set
        {
            field = value;
            EvaluateGoal();
        }
    }

    /// <summary>
    /// Opening balance for this Fund Goal
    /// </summary>
    public decimal OpeningBalance
    {
        get;
        internal set
        {
            field = value;
            EvaluateGoal();
        }
    }

    /// <summary>
    /// Amount assigned for this Fund Goal
    /// </summary>
    public decimal AmountAssigned
    {
        get;
        internal set
        {
            field = value;
            EvaluateGoal();
        }
    }

    /// <summary>
    /// Pending amount assigned for this Fund Goal
    /// </summary>
    public decimal PendingAmountAssigned
    {
        get;
        internal set
        {
            field = value;
            EvaluateGoal();
        }
    }

    /// <summary>
    /// Remaining amount to assign for this Fund Goal
    /// </summary>
    public decimal RemainingAmountToAssign { get; private set; }

    /// <summary>
    /// Indicates whether the assignment goal has been met for this Fund Goal
    /// </summary>
    public bool IsAssignmentGoalMet { get; private set; }

    /// <summary>
    /// Amount spent for this Fund Goal
    /// </summary>
    public decimal AmountSpent
    {
        get;
        internal set
        {
            field = value;
            EvaluateGoal();
        }
    }

    /// <summary>
    /// Pending amount spent for this Fund Goal
    /// </summary>
    public decimal PendingAmountSpent
    {
        get;
        internal set
        {
            field = value;
            EvaluateGoal();
        }
    }

    /// <summary>
    /// Remaining amount to spend for this Fund Goal
    /// </summary>
    public decimal RemainingAmountToSpend { get; private set; }

    /// <summary>
    /// Indicates whether the spending goal has been met for this Fund Goal
    /// </summary>
    public bool IsSpendingGoalMet { get; private set; }

    /// <summary>
    /// Closing balance for this Fund Goal
    /// </summary>
    public decimal ClosingBalance
    {
        get;
        internal set
        {
            field = value;
            EvaluateGoal();
        }
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundGoal(
        Fund fund,
        AccountingPeriodId accountingPeriodId,
        FundGoalType goalType,
        decimal goalAmount,
        decimal openingBalance,
        decimal amountAssigned,
        decimal pendingAmountAssigned,
        decimal amountSpent,
        decimal pendingAmountSpent,
        decimal closingBalance)
        : base(new FundGoalId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriodId = accountingPeriodId;
        GoalType = goalType;
        GoalAmount = goalAmount;
        OpeningBalance = openingBalance;
        AmountAssigned = amountAssigned;
        PendingAmountAssigned = pendingAmountAssigned;
        AmountSpent = amountSpent;
        PendingAmountSpent = pendingAmountSpent;
        ClosingBalance = closingBalance;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private FundGoal()
        : base()
    {
        Fund = null!;
        AccountingPeriodId = null!;
        GoalType = default;
    }

    /// <summary>
    /// Evaluates progress towards the goal
    /// </summary>
    private void EvaluateGoal()
    {
        if (GoalType == FundGoalType.Monthly)
        {
            RemainingAmountToAssign = GoalAmount - OpeningBalance - AmountAssigned;
            RemainingAmountToSpend = -1 * ClosingBalance;
        }
        else if (GoalType == FundGoalType.Rolling)
        {
            RemainingAmountToAssign = GoalAmount - AmountAssigned;
            RemainingAmountToSpend = -1 * ClosingBalance;
        }
        else if (GoalType == FundGoalType.Savings)
        {
            RemainingAmountToAssign = GoalAmount - AmountAssigned;
            RemainingAmountToSpend = 0;
        }
        else if (GoalType == FundGoalType.Debt)
        {
            RemainingAmountToAssign = GoalAmount - AmountAssigned;
            RemainingAmountToSpend = ClosingBalance;
        }
        IsAssignmentGoalMet = RemainingAmountToAssign >= 0;
        IsSpendingGoalMet = RemainingAmountToSpend <= 0;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="FundGoal"/>
/// </summary>
public record FundGoalId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundGoalId(Guid value) : base(value) { }
}
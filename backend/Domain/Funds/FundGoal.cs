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
    public FundGoalType GoalType { get; internal set; }

    /// <summary>
    /// Target amount for this Fund Goal
    /// </summary>
    public decimal GoalAmount { get; internal set; }

    /// <summary>
    /// Whether the goal has been met for this Fund Goal
    /// </summary>
    public bool IsGoalMet { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundGoal(Fund fund, AccountingPeriodId accountingPeriodId, FundGoalType goalType, decimal goalAmount)
        : base(new FundGoalId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriodId = accountingPeriodId;
        GoalType = goalType;
        GoalAmount = goalAmount;
        IsGoalMet = false;
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
using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Entity class representing the spending goal for a Fund within a particular Accounting Period.
/// </summary>
public class SpendingGoal : Entity<SpendingGoalId>
{
    /// <summary>
    /// Fund for this Spending Goal
    /// </summary>
    public Fund Fund { get; private set; }

    /// <summary>
    /// Accounting Period ID for this Spending Goal
    /// </summary>
    public AccountingPeriodId? AccountingPeriodId { get; private set; }

    /// <summary>
    /// Type for this Spending Goal
    /// </summary>
    public SpendingGoalType SpendingGoalType { get; private set; }

    /// <summary>
    /// Total amount to spend for this Spending Goal
    /// </summary>
    public decimal TotalAmountToSpend { get; private set; }

    /// <summary>
    /// Total amount spent for this Spending Goal
    /// </summary>
    public decimal TotalAmountSpent { get; private set; }

    /// <summary>
    /// Total amount spent for this Spending Goal including pending spent amounts
    /// </summary>
    public decimal TotalAmountSpentIncludingPending { get; private set; }

    /// <summary>
    /// Indicates whether the spending goal has been met
    /// </summary>
    public bool IsGoalMet { get; private set; }

    /// <summary>
    /// Indicates whether the spending goal has been met including pending spent amounts
    /// </summary>
    public bool IsGoalMetIncludingPending { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal SpendingGoal(
        Fund fund,
        AccountingPeriodId? accountingPeriodId,
        SpendingGoalType spendingGoalType)
        : base(new SpendingGoalId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriodId = accountingPeriodId;
        SpendingGoalType = spendingGoalType;
    }

    /// <summary>
    /// Updates this Spending Goal
    /// </summary>
    internal void UpdateGoal(SpendingGoalType spendingGoalType, AccountingPeriodFundBalanceHistory balanceHistory)
    {
        SpendingGoalType = spendingGoalType;
        EvaluateGoal(balanceHistory);
    }

    /// <summary>
    /// Evaluates progress towards the spending goal
    /// </summary>
    internal void EvaluateGoal(AccountingPeriodFundBalanceHistory balanceHistory)
    {
        TotalAmountToSpend = balanceHistory.OpeningBalance + balanceHistory.AmountAssigned;
        TotalAmountSpent = balanceHistory.AmountSpent;
        TotalAmountSpentIncludingPending = TotalAmountSpent + balanceHistory.PendingAmountSpent;

        IsGoalMet = SpendingGoalType switch
        {
            SpendingGoalType.Standard => TotalAmountSpent <= TotalAmountToSpend,
            SpendingGoalType.Debt => TotalAmountSpent == TotalAmountToSpend,
            _ => throw new InvalidOperationException($"Unsupported spending goal type '{SpendingGoalType}'."),
        };
        IsGoalMetIncludingPending = SpendingGoalType switch
        {
            SpendingGoalType.Standard => TotalAmountSpentIncludingPending <= TotalAmountToSpend,
            SpendingGoalType.Debt => TotalAmountSpentIncludingPending == TotalAmountToSpend,
            _ => throw new InvalidOperationException($"Unsupported spending goal type '{SpendingGoalType}'."),
        };
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private SpendingGoal()
        : base()
    {
        Fund = null!;
        AccountingPeriodId = null;
        SpendingGoalType = default;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="SpendingGoal"/>
/// </summary>
public record SpendingGoalId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal SpendingGoalId(Guid value) : base(value) { }
}
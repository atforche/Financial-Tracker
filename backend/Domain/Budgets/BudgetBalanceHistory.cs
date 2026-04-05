using Domain.Transactions;

namespace Domain.Budgets;

/// <summary>
/// Entity class representing a point-in-time snapshot of a Budget balance within a particular accounting period.
/// </summary>
public class BudgetBalanceHistory : Entity<BudgetBalanceHistoryId>
{
    /// <summary>
    /// Budget Goal that owns this balance history entry
    /// </summary>
    public BudgetGoal BudgetGoal { get; init; }

    /// <summary>
    /// Transaction ID that caused this balance change
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Date of this balance history entry
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Ordering sequence for entries on the same date
    /// </summary>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Posted balance at this point in time
    /// </summary>
    public decimal PostedBalance { get; init; }

    /// <summary>
    /// Available to spend at this point in time, or null if not applicable for this budget type
    /// </summary>
    public decimal? AvailableToSpend { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal BudgetBalanceHistory(BudgetGoal budgetGoal, TransactionId transactionId, DateOnly date, int sequence,
        decimal postedBalance, decimal? availableToSpend)
        : base(new BudgetBalanceHistoryId(Guid.NewGuid()))
    {
        BudgetGoal = budgetGoal;
        TransactionId = transactionId;
        Date = date;
        Sequence = sequence;
        PostedBalance = postedBalance;
        AvailableToSpend = availableToSpend;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private BudgetBalanceHistory()
        : base()
    {
        BudgetGoal = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="BudgetBalanceHistory"/>
/// </summary>
public record BudgetBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal BudgetBalanceHistoryId(Guid value) : base(value) { }
}

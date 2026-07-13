using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.Goals;

/// <summary>
/// Entity class representing the balance of a Goal after a Transaction.
/// </summary>
public class GoalBalanceHistory : Entity<GoalBalanceHistoryId>
{
    /// <summary>
    /// Fund ID for this Goal Balance History.
    /// </summary>
    public FundId FundId { get; init; }

    /// <summary>
    /// Accounting Period ID for this Goal Balance History.
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Transaction ID for this Goal Balance History.
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Date for this Goal Balance History.
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Sequence number for this Goal Balance History.
    /// </summary>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Amount assigned for this Goal Balance History.
    /// </summary>
    public decimal AmountAssigned { get; private set; }

    /// <summary>
    /// Pending amount assigned for this Goal Balance History.
    /// </summary>
    public decimal PendingAmountAssigned { get; private set; }

    /// <summary>
    /// Amount spent for this Goal Balance History.
    /// </summary>
    public decimal AmountSpent { get; private set; }

    /// <summary>
    /// Pending amount spent for this Goal Balance History.
    /// </summary>
    public decimal PendingAmountSpent { get; private set; }

    /// <summary>
    /// Updates this Goal Balance History with a new Goal Balance.
    /// </summary>
    internal void Update(GoalBalance goalBalance)
    {
        if (goalBalance.FundId != FundId)
        {
            throw new InvalidOperationException("Cannot update Goal Balance History with a Goal Balance for a different Fund");
        }
        AmountAssigned = goalBalance.AmountAssigned;
        PendingAmountAssigned = goalBalance.PendingAmountAssigned;
        AmountSpent = goalBalance.AmountSpent;
        PendingAmountSpent = goalBalance.PendingAmountSpent;
    }

    /// <summary>
    /// Converts this Goal Balance History to a Goal Balance.
    /// </summary>
    public GoalBalance ToGoalBalance() => new(FundId, AmountAssigned, PendingAmountAssigned, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal GoalBalanceHistory(FundId fundId, AccountingPeriodId accountingPeriodId, TransactionId transactionId, DateOnly date, int sequence, GoalBalance goalBalance)
        : base(new GoalBalanceHistoryId(Guid.NewGuid()))
    {
        FundId = fundId;
        AccountingPeriodId = accountingPeriodId;
        TransactionId = transactionId;
        Date = date;
        Sequence = sequence;
        Update(goalBalance);
    }

    /// <summary>
    /// Constructs a new default instance of this class.
    /// </summary>
    private GoalBalanceHistory()
    {
        FundId = null!;
        AccountingPeriodId = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="GoalBalanceHistory"/>.
/// </summary>
public record GoalBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal GoalBalanceHistoryId(Guid value) : base(value) { }
}
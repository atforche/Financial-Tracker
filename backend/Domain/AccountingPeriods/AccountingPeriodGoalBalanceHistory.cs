using Domain.Funds;
using Domain.Goals;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balance of a Goal within an Accounting Period.
/// </summary>
public class AccountingPeriodGoalBalanceHistory : Entity<AccountingPeriodGoalBalanceHistoryId>
{
    /// <summary>
    /// Fund for this Accounting Period Goal Balance History.
    /// </summary>
    public Fund Fund { get; init; }

    /// <summary>
    /// Accounting Period for this Accounting Period Goal Balance History.
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Amount assigned during the Accounting Period.
    /// </summary>
    public decimal AmountAssigned { get; private set; }

    /// <summary>
    /// Pending amount assigned during the Accounting Period.
    /// </summary>
    public decimal PendingAmountAssigned { get; private set; }

    /// <summary>
    /// Amount spent during the Accounting Period.
    /// </summary>
    public decimal AmountSpent { get; private set; }

    /// <summary>
    /// Pending amount spent during the Accounting Period.
    /// </summary>
    public decimal PendingAmountSpent { get; private set; }

    /// <summary>
    /// Gets the Goal Balance for this Accounting Period Goal Balance History.
    /// </summary>
    public GoalBalance GetGoalBalance() => new(Fund.Id, AmountAssigned, PendingAmountAssigned, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Updates this Accounting Period Goal Balance History.
    /// </summary>
    internal void Update(GoalBalance goalBalance)
    {
        AmountAssigned = goalBalance.AmountAssigned;
        PendingAmountAssigned = goalBalance.PendingAmountAssigned;
        AmountSpent = goalBalance.AmountSpent;
        PendingAmountSpent = goalBalance.PendingAmountSpent;
    }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountingPeriodGoalBalanceHistory(Fund fund, AccountingPeriod accountingPeriod, GoalBalance goalBalance)
        : base(new AccountingPeriodGoalBalanceHistoryId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriod = accountingPeriod;
        Update(goalBalance);
    }

    /// <summary>
    /// Constructs a new default instance of this class.
    /// </summary>
    private AccountingPeriodGoalBalanceHistory()
    {
        Fund = null!;
        AccountingPeriod = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountingPeriodGoalBalanceHistory"/>.
/// </summary>
public record AccountingPeriodGoalBalanceHistoryId : EntityId
{
    /// <summary>Constructs a new instance of this class.</summary>
    internal AccountingPeriodGoalBalanceHistoryId(Guid value) : base(value) { }
}
using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.AccountGoals;

/// <summary>
/// Entity class representing the goal for an Account in an Accounting Period.
/// </summary>
public sealed class AccountGoal : Entity<AccountGoalId>
{
    /// <summary>
    /// Account associated with this Account Goal.
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Accounting Period associated with this Account Goal, or null for an onboarded Account Goal.
    /// </summary>
    public AccountingPeriod? AccountingPeriod { get; private set; }

    /// <summary>
    /// Minimum desired ending balance.
    /// </summary>
    public decimal? MinimumEndingBalance { get; private set; }

    /// <summary>
    /// Maximum desired ending balance.
    /// </summary>
    public decimal? MaximumEndingBalance { get; private set; }

    /// <summary>
    /// Updates the configurable quantities for this Account Goal.
    /// </summary>
    internal void Update(decimal? minimumEndingBalance, decimal? maximumEndingBalance)
    {
        MinimumEndingBalance = minimumEndingBalance;
        MaximumEndingBalance = maximumEndingBalance;
    }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountGoal(
        Account account,
        AccountingPeriod? accountingPeriod,
        decimal? minimumEndingBalance,
        decimal? maximumEndingBalance)
        : base(new AccountGoalId(Guid.NewGuid()))
    {
        Account = account;
        AccountingPeriod = accountingPeriod;
        Update(minimumEndingBalance, maximumEndingBalance);
    }

    /// <summary>
    /// Constructs a new default instance of this class.
    /// </summary>
    private AccountGoal()
    {
        Account = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountGoal"/>.
/// </summary>
public sealed record AccountGoalId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountGoalId(Guid value) : base(value) { }
}

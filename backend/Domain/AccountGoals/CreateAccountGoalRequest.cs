using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.AccountGoals;

/// <summary>
/// Record representing a request to create an <see cref="AccountGoal"/>.
/// </summary>
public sealed record CreateAccountGoalRequest
{
    /// <summary>
    /// Account associated with the Account Goal.
    /// </summary>
    public Account Account { get; init; } = null!;

    /// <summary>
    /// Accounting Period associated with the Account Goal, or null for an onboarded Account Goal.
    /// </summary>
    public AccountingPeriod? AccountingPeriod { get; init; }

    /// <summary>
    /// Minimum desired ending balance.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Maximum desired ending balance.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}

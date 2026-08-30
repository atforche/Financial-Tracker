using Models.AccountingPeriods;
using Models.Accounts;

namespace Models.AccountGoals;

/// <summary>
/// Model representing an Account Goal for an Accounting Period.
/// </summary>
public sealed class AccountGoalModel
{
    /// <summary>
    /// Gets the Account Goal ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Account associated with the Account Goal.
    /// </summary>
    public required AccountModel Account { get; init; }

    /// <summary>
    /// Gets the associated Accounting Period, or null for an onboarded Account Goal.
    /// </summary>
    public AccountingPeriodModel? AccountingPeriod { get; init; }

    /// <summary>
    /// Gets the minimum desired ending balance.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Gets the maximum desired ending balance.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}

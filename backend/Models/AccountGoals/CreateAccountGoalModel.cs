namespace Models.AccountGoals;

/// <summary>
/// Model representing Account Goal configuration for creation.
/// </summary>
public sealed class CreateAccountGoalModel
{
    /// <summary>
    /// Gets the Account ID.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Gets the Accounting Period ID, or null for an onboarded Account Goal.
    /// </summary>
    public Guid? AccountingPeriodId { get; init; }

    /// <summary>
    /// Gets the minimum desired ending balance.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Gets the maximum desired ending balance.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}

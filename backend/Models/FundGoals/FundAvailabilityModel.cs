namespace Models.FundGoals;

/// <summary>
/// Model describing the current availability of a Fund.
/// </summary>
public sealed class FundAvailabilityModel
{
    /// <summary>
    /// Gets the posted available balance.
    /// </summary>
    public required decimal AvailableBalance { get; init; }

    /// <summary>
    /// Gets the available balance including pending activity.
    /// </summary>
    public required decimal AvailableBalanceIncludingPending { get; init; }

    /// <summary>
    /// Gets whether the posted balance is overspent.
    /// </summary>
    public required bool IsOverspent { get; init; }

    /// <summary>
    /// Gets whether the balance including pending activity is overspent.
    /// </summary>
    public required bool IsOverspentIncludingPending { get; init; }
}
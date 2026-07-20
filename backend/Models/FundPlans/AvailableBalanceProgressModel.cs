namespace Models.FundPlans;

/// <summary>
/// Model describing a Fund's available-balance health.
/// </summary>
public sealed class AvailableBalanceProgressModel
{
    /// <summary>
    /// Gets the current available balance.
    /// </summary>
    public required decimal CurrentBalance { get; init; }

    /// <summary>
    /// Gets the minimum allowed available balance.
    /// </summary>
    public required decimal MinimumBalance { get; init; }

    /// <summary>
    /// Gets the amount required to restore the minimum allowed balance.
    /// </summary>
    public required decimal Shortfall { get; init; }

    /// <summary>
    /// Gets whether the available balance is at least zero.
    /// </summary>
    public required bool IsSatisfied { get; init; }
}
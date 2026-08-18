namespace Models.FundGoals;

/// <summary>
/// Model describing post-assignment funded-balance progress.
/// </summary>
public sealed class FundedBalanceProgressModel
{
    /// <summary>
    /// Gets the funded balance.
    /// </summary>
    public required decimal Balance { get; init; }

    /// <summary>
    /// Gets the configured minimum balance.
    /// </summary>
    public decimal? MinimumBalance { get; init; }

    /// <summary>
    /// Gets the configured maximum balance.
    /// </summary>
    public decimal? MaximumBalance { get; init; }

    /// <summary>
    /// Gets the amount below the minimum.
    /// </summary>
    public required decimal AmountBelowMinimum { get; init; }

    /// <summary>
    /// Gets the amount above the maximum.
    /// </summary>
    public required decimal AmountAboveMaximum { get; init; }

    /// <summary>
    /// Gets the funded-balance status.
    /// </summary>
    public required FundedBalanceStatusModel Status { get; init; }
}
